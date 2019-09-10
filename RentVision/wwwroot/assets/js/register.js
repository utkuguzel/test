var special = /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/;
var uppercase = /[A-Z]/;

function init() {
    $("#password, #confirmPassword").on("keyup paste", onInputChanged);
}

function onInputChanged(e) {
    var password = $("#password").val();
    var succes = true;

    $(".passwordRequirements i").addClass("fa-check").removeClass("fa-times");

    if (password.length < 8) {
        $(".minLengthRequirement > i").addClass("fa-times").removeClass("fa-check");
        succes = false;
    }

    if (!hasNumber(password)) {
        $(".numberRequirement > i").addClass("fa-times").removeClass("fa-check");
        succes = false;
    }

    if (!uppercase.test(password)) {
        $(".upperCaseRequirement > i").addClass("fa-times").removeClass("fa-check");
        succes = false;
    }

    if (!special.test(password)) {
        $(".specialCharRequirement > i").addClass("fa-times").removeClass("fa-check");
        succes = false;
    }

    if ($("#password").val() !== $("#confirmPassword").val()) {
        $(".passwordMatchRequirement > i").addClass("fa-times").removeClass("fa-check");
        succes = false;
    }

    if (succes) {
        $("button[disabled]").removeAttr("disabled");
    }
    else {
        $("button").attr("disabled");
    }
}

function hasNumber(myString) {
    return /\d/.test(myString);
}

$(document).ready(init);