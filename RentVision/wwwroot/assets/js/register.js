var special = /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/;
var uppercase = /[A-Z]/;

function init() {
    $(".passwordRequirements").hide();
    $("#password, #confirmPassword").on("keyup paste", onInputChanged);
    $("#password, #confirmPassword").on("focus", showPasswordRequirements);
}

function showPasswordRequirements() {
    if (!$(".passwordRequirements").is(":visible")) {
        $(".passwordRequirements").slideDown();
    }
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
        $(".passwordRequirements").addClass("success").removeClass("error");
    }
    else {
        $("button").attr("disabled");
        $(".passwordRequirements").removeClass("success").addClass("error");
    }

    showPasswordRequirements();
}

function hasNumber(myString) {
    return /\d/.test(myString);
}

$(document).ready(init);