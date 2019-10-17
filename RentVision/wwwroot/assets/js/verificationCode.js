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
                body.find('input').eq(i).val(pastedData[i]);
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
            code += body.find('input').eq(i).val();
        }

        $.post("/account/verify/code/" + email + "/" + code, function (response) {
            if (response.statusCode === 200) {
                $(".verificationForm").removeClass("error").addClass("success");

                setTimeout(function () {
                    $('.wizard').wizard('next');
                }, 1000);
            }
            else {
                $(".verificationForm").removeClass("success").addClass("error");
            }
        });
    }

    function checkPayment() {
        // TODO: Check payment status here
    }

    $('.wizard').on('actionclicked.fu.wizard', function (_, data) {
        if (data.step === 1 && data.direction === "next") {
            var planFree = $(".step-pane-payment").data("skip");

            if (planFree !== "True") {
                checkPayment();
            }
            else if ( planFree === "True" ) {
                $(".step-pane-payment").data("skip", "");
                $(".wizard").wizard("next");
                startSetupPoll();
            }
        }
        else if (data.step === 2 && data.direction === "next") {
            startSetupPoll();
        }
    });

    // Check if user is already verified
    $.post("/account/verify/" + $(".setup").data("email"), function (response) {
        var responseObject = JSON.parse(response.responseString);
        if (responseObject.value === true) {
            console.log("Skip verification");
            $('.wizard').wizard('next');
            $(".verificationForm").removeClass("error").addClass("success");
        }
        else {
            // Check if there is a code set in the code attribute
            checkForExistingCode();
        }
    });

    function checkForExistingCode() {
        var existingCode = $(".verificationForm").data("code");

        if (existingCode !== undefined) {
            var strExistingCode = existingCode.toString();

            for (var i = 0; i < 4; i++) {
                body.find('input').eq(i).val(strExistingCode[i]);
            }

            verifyCode();
        }
    }

    $(".setup-success").hide();
    $(".setup-error").hide();

    body.on('keyup', 'input', goToNextInput);
    body.on('keydown paste', 'input', onKeyDown);
    body.on('click', 'input', onFocus);
});