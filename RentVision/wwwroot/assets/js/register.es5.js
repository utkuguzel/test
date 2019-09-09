"use strict";

var email;
var redirectUrl;
var businessUnitName;

function init() {
    email = $(".setup").data("email");
    redirectUrl = $(".setup").data("redirect");

    $(".timeOutMessage").hide();

    if (email !== undefined) {
        isUserSiteReady();
        setTimeout(timeOutCheck, 60000);
    }
}

function timeOutCheck() {
    $(".setup-content > .timeOutMessage").slideDown(250);
}

function isUserSiteReady() {
    $.ajax({
        method: "POST",
        url: "/auth/isUserSiteReady",
        dataType: "json",
        data: { email: email },
        success: onSuccessCallBack,
        error: onErrorCallBack
    });

    setTimeout(isUserSiteReady, 1000);
}

function onSuccessCallBack(data) {

    if (data.response === "true" && data.statusCode === 200) {
        if (redirectUrl !== undefined) {
            window.location.href = redirectUrl;
        } else {
            console.log("Failed to redirect. Redirect URL not set.");
        }
    }

    console.log(data);
}

function onErrorCallBack(jqXhr, error, errorStr) {
    console.log(error + ": " + errorStr);
}

$(document).ready(init);

