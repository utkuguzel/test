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
        //TODO: Ajax call aanroepen hier

        $(".verificationForm").addClass("success");

        setTimeout(function () {
            $('.wizard').wizard('next');
        }, 1000);
    }

    $('.wizard').on('actionclicked.fu.wizard', function (evt, data) {
        if (data.step === 2 && data.direction === "next") {
            startSetupPoll();
        }
    });

    body.on('keyup', 'input', goToNextInput);
    body.on('keydown paste', 'input', onKeyDown);
    body.on('click', 'input', onFocus);
});