var email;
var timer;
var nopes = 0;

function startSetupPoll()
{
    email = $(".setup").data("email");

    $(".timeOutMessage").hide();

    isUserSiteReady();
    setTimeout(timeOutCheck, 60000);
}

function timeOutCheck()
{
    $(".setup-content > .timeOutMessage").slideDown(250);
}

function isUserSiteReady()
{
    $.ajax({
        method: "POST",
        url: "/auth/isUserSiteReady",
        dataType: "json",
        data: { email: email },
        success: onSuccessCallBack,
        error: onErrorCallBack
    });

    console.log("IS USER SITE READY");

    timer = setTimeout(isUserSiteReady, 1000);
}

function onSuccessCallBack(response)
{  
    if (response.response === "true" && response.statusCode === 200)
    {
        clearTimeout(timer);

        $.ajax({
            method: "POST",
            url: "/auth/getUserKey",
            dataType: "json",
            success: onUserKeySuccessCallBack,
            error: onUserKeyErrorCallBack
        });
    }
    else if (response.statusCode !== 200)
    {
        $(".setup-working").fadeOut("fast", function () {
            $(".setup-error").fadeIn("fast");
        });
    }
    else if (response.response === "false")
    {
        nopes += 1;

        if (nopes >= 30) {
            clearTimeout(timer);
            $(".setup-working").fadeOut("fast", function () {
                $(".setup-error").fadeIn("fast");
            });
        }
    }
}

function onUserKeySuccessCallBack(data)
{
    window.location.href = data.realRedirectUrl;
}

function onUserKeyErrorCallBack(jqXhr, error, errorStr)
{
    console.log(error + ": " + errorStr);
}

function onErrorCallBack(jqXhr, error, errorStr)
{
    console.log(error + ": " + errorStr);
}

$(document).ready(function () {
    $("#modal-button-skip").on("click", function () {
        console.log("Skip payment trigger AJAX call set to free plan and continue");
    });
});