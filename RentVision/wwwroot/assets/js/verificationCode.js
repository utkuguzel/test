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

    // 17 = CTRL, 86 = V
    function onKeyDown(e) {
        var key = e.which;

        // Handle on input paste
        if (e.type === 'paste') {
            var pastedData = e.originalEvent.clipboardData.getData('text');

            for (var i = 0; i < 4; i++) {
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
    }

    function verifyCode() {
        var code = "";
        var email = $(".setup").data("email");
        
        for (var i = 0; i < 4; i++) {
            code += $(".verificationForm").find('input').eq(i).val();
        }

        $.post("/verify/code/" + email + "/" + code, function (response) {
            if (response.statusCode === 200) {
                $(".verificationForm").removeClass("error").addClass("success");
                $("#CHECK").addClass("active");

                setTimeout(function () {
                    console.log("(verifyCode) Attempt to go to the next step");
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

        if (checkoutUrl === "True") {
            $(".wizard").wizard("selectedItem", { step: 4 });
        }
        else {
            setTimeout(checkMolliePaymentStatus, 1000);
        }
    }

    function checkMolliePaymentStatus() {
        var molliePaymentId = $(".step-pane-payment").data("payment-id");

        console.log(molliePaymentId);

        if (molliePaymentId !== undefined) {
            $.post("/verify/transaction/" + molliePaymentId, function (response) {
                if (response.statusCode === 200 && response.paymentStatus === "Paid") {
                    $('.wizard').wizard('selectedItem', { step: 4 });
                }
            });
        }

        setTimeout(checkMolliePaymentStatus, 1000);
    }

    $('.wizard').on('changed.fu.wizard', function (_, data) {
        if (data.step === 3) {
            var planFree = $(".step-pane-payment").data("skip");
            
            if (planFree !== "True") {
                checkPayment();
            }
            else if (planFree === "True") {
                console.log("Skip 2");
                $(".step-pane-payment").data("skip", "");
                $(".wizard").wizard("selectedItem", { step: 4 });
                startSetupPoll();
            }
        }

        if (data.step === 4) {
            startSetupPoll();
        }

        console.log(data.step);
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
            else {
                console.log(response.statusMessage);
            }
        });
    }

    function checkForExistingCode() {
        var existingCode = $(".verificationForm").data("code");

        if (existingCode !== undefined) {
            var strExistingCode = existingCode.toString();

            for (var i = 0; i < 4; i++) {
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
});