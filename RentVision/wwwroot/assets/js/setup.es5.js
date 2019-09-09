"use strict";

var email;
var businessUnitName;

console.log("SCRIPT");

function init() {
    email = $(".setup").data("email");

    $(".timeOutMessage").hide();

    isUserSiteReady();
    setTimeout(timeOutCheck, 60000);

    console.log(email);
}

function timeOutCheck() {
    $(".setup-content > .timeOutMessage").slideDown(250);
}

function isUserSiteReady() {
    console.log("isUserSiteReady");

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
        console.log("onSuccessCallBack");

        $.ajax({
            method: "POST",
            url: "/auth/getUserKey",
            dataType: "json",
            data: { email: email },
            success: onUserKeySuccessCallBack,
            error: onUserKeyErrorCallBack
        });
    } else if (data.statusCode !== 200) {
        $(".setup-content > i").hide();
        $(".setup-content > .timeOutMessage").hide();
        $(".setup-content > .loadingText").text("An unknown error occured, please try again. If this problem persists, please contact technical support.");
    }

    console.log(data);
}

function onUserKeySuccessCallBack(data) {
    console.log(data);
}

function onUserKeyErrorCallBack(jqXhr, error, errorStr) {
    console.log(error + ": " + errorStr);
}

function onErrorCallBack(jqXhr, error, errorStr) {
    console.log(error + ": " + errorStr);
}

$(document).ready(init);

