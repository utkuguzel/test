$(function () {
    'use strict';

    var body = $('body');

    function goToNextInput(e) {
        var key = e.which,
            t = $(e.target),
            sib = t.next('input');

        if (key !== 9 && (key < 48 || key > 57)) {
            e.preventDefault();
            return false;
        }

        if (key === 9) {
            return true;
        }

        if (!sib || !sib.length) {
            sib = body.find('input').eq(0);
            verifyCode();
        }

        sib.select().focus();
    }

    function goToPreviousInput(e) {
        var t = $(e.target),
            sib = t.prev('input');

        if (!sib || !sib.length) {
            sib = body.find('input').eq(0);
        }

        sib.select().focus();
    }

    // 17 = CTRL, 86 = V
    function onKeyDown(e) {
        var key = e.which;

        // Handle backspace
        if (key === 8) {
            goToPreviousInput(e);
            return true;
        }

        // Handle on input paste
        if (e.type === 'paste') {
            var pastedData = e.originalEvent.clipboardData.getData('text');
            var codeInputLength = $(".verificationForm").find("input").length;

            for (var i = 0; i < codeInputLength; i++) {
                $(".verificationForm").find('input').eq(i).val(pastedData[i]);
            }

            verifyCode();
        }

        if (key === 9 || key === 17 || key === 86 || (key >= 48 && key <= 57)) {
            return true;
        }

        e.preventDefault();
        return false;
    }

    function onFocus(e) {
        $(e.target).select();
        $(".verificationForm input.focus").focus();
    }

    function verifyCode() {
        var code = "";
        var email = $(".setup").data("email");
        var codeInputLength = $(".verificationForm").find("input").length;
        
        for (var i = 0; i < codeInputLength; i++) {
            code += $(".verificationForm").find('input').eq(i).val();
        }

        $.post("/verify/code/" + email + "/" + code, function (response) {
            if (response.statusCode === 200) {
                $(".verificationForm").removeClass("error").addClass("success");
                $("#CHECK").addClass("active");

                setTimeout(function () {
                    $('.wizard').wizard('selectedItem', { step: 3 });
                }, 1250);
            }
            else {
                $(".verificationForm").removeClass("success").addClass("error");
            }
        });
    }

    function checkPayment() {
        var checkoutUrl = $("#checkoutButton").data("skip");
        var paymentError = $(".step-pane-payment").data("error");

        if (checkoutUrl === "True" && paymentError === "False") {
            $(".wizard").wizard("selectedItem", { step: 4 });
        }
        else {
            setTimeout(checkMolliePaymentStatus, 2000);
        }
    }

    function checkMolliePaymentStatus() {
        var molliePaymentId = $(".step-pane-payment").data("payment-id");

        if (molliePaymentId !== undefined) {
            $.post("/verify/transaction/" + molliePaymentId, function (response) {
                if (response.statusCode === 200 && response.paymentStatus === "Paid") {
                    $('.wizard').wizard('selectedItem', { step: 4 });
                }

                console.log(response);
            });
        }

        setTimeout(checkMolliePaymentStatus, 2000);
    }

    $('.wizard').on('changed.fu.wizard', function (_, data) {
        if (data.step === 3) {
            var planFree = $(".step-pane-payment").data("skip");
            
            if (planFree !== "True") {
                checkPayment();
            }
            else if (planFree === "True") {
                $(".step-pane-payment").data("skip", "");
                $(".wizard").wizard("selectedItem", { step: 4 });
            }
        }

        if (data.step === 4) {
            startSetupPoll();
            //console.log("Check");
        }
    });

    // Check if user is already verified
    $.post("/verify/code/" + $(".setup").data("email"), function (response) {
        if (response.statusCode === 200) {
            $('.wizard').wizard('selectedItem', { step: 3 });
            checkPayment();
        }
        else if (response.statusCode === 401) {
            $('.wizard').wizard('selectedItem', { step: 2 });
            $(".email-working").hide();
            $(".verification-box").show();
            checkForExistingCode();
        }
        else {
            createVerificationCodeEmail();
        }
    });

    function createVerificationCodeEmail() {
        $.post("/verify/createVerificationCodeEmail/" + $(".setup").data("email"), function (response) {
            if (response.statusCode === 200) {
                $(".email-working").fadeOut("fast", function () {
                    $('.wizard').wizard('selectedItem', { step: 2 });
                    $(".verification-box").fadeIn("fast");
                });
            }
        });
    }

    function resendVerificationCode() {
        $('.wizard').wizard('selectedItem', { step: 1 });
        $(".email-working").show();
        $(".verification-box").hide();
        createVerificationCodeEmail();
    }

    function checkForExistingCode() {
        var existingCode = $(".verificationForm").data("code");

        if (existingCode !== undefined) {
            var strExistingCode = existingCode.toString();
            var codeInputLength = $(".verificationForm").find("input").length;

            for (var i = 0; i < codeInputLength; i++) {
                $(".verificationForm").find('input').eq(i).val(strExistingCode[i]);
            }

            verifyCode();
        }
        else {
            return false;
        }
    }

    $(".setup-success").hide();
    $(".setup-error").hide();
    $(".verification-box").hide();

    body.on('keyup', 'input', goToNextInput);
    body.on('keydown paste', 'input', onKeyDown);
    body.on('click', 'input', onFocus);

    $(".resend > a").on("click", resendVerificationCode);
});