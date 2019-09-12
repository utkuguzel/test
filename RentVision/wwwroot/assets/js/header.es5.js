"use strict";

function init() {
    // Init header hover
    $(".headerHoverBar").slideUp();

    // This function resets certain elements to their default state (hidden) if another element that was not in their parent container was clicked
    // To prevent a messy header
    $(document).on("mousedown touchend", function (e) {
        if ($(e.target).closest("#rightmenu-mobile").length === 0 && $("#rightmenu-mobile").is(":visible") && $(e.target).closest("#rightmenu").length === 0) {
            $("#rightmenu-mobile").find(".fa").removeClass("fa-times").addClass("fa-bars");
            $("#rightmenu").stop().fadeOut(150);
        }
    });

    // Fix for resizing client window, this will reset the mobile menu on resize
    $(window).resize(function () {
        if ($(this).width() > 992) {
            $("#rightmenu").show();
        } else {
            $("#rightmenu").hide();
            $("#rightmenu-mobile").find(".fa").removeClass("fa-times").addClass("fa-bars");
            $("#rightmenu").stop().fadeOut(150);

            // Hide all visible dropdowns too
            $(".headerDropdown").hide();
        }
    });

    // This function handles hamburger menu clicks/touch
    $("#rightmenu-mobile").on("click", function (e) {
        //e.preventDefault();

        var icon = $(this).find(".fa");

        if (icon.hasClass("fa-bars")) {
            $(this).find(".fa").removeClass("fa-bars").addClass("fa-times");
            $("#rightmenu").stop().fadeIn(200);
        } else {
            $(this).find(".fa").removeClass("fa-times").addClass("fa-bars");
            $("#rightmenu").stop().fadeOut(150);
        }
    });

    $(".headerOption").on("click", function (e) {
        var dropdown = $(this).find(".headerDropdown");

        if (dropdown && !dropdown.is(":visible")) {
            //$(this).find("i").removeClass("fa-angle-down").addClass("fa-angle-up");
            dropdown.show();
        } else if (dropdown && dropdown.is(":visible")) {
            if (!$(e.target).hasClass(".headerDropdown")) {
                //$(this).find("i").removeClass("fa-angle-up").addClass("fa-angle-down");
                dropdown.hide();
            }
        }
    });

    $(".headerOption").on("mouseleave", function (e) {
        var dropdown = $(this).find(".headerDropdown");

        if (dropdown && $(window).width() > 992) {
            //$(this).find("i").removeClass("fa-angle-up").addClass("fa-angle-down");
            dropdown.hide();
        }
    });

    //$(".headerOption").on("mousedown touchend", function (e)
    //{
    //    e.preventDefault();

    //    var dropdown = $(this).find(".headerDropdown");

    //    if (dropdown.is(":visible") && $(window).width() <= 992 )
    //    {
    //        dropdown.hide();
    //    }
    //});

    // Handle green hover bar above header option
    $(".headerOption").on("mouseenter touchend", function (e) {
        $(this).find(".headerHoverBar").stop().slideDown(100);
    });

    // Handles green hover bar animation above header option
    $(".headerOption").on("mouseleave touchend", function (e) {
        $(this).find(".headerHoverBar").stop().slideUp(100);
    });

    // Error modal events
    $("#errorModal").on("mousedown touchend", function () {
        $(this).stop().fadeOut(250);
    });
}

$("body").scroll(function (e) {
    var top = this.scrollTop;

    //console.log(top);

    if (top > 70) {
        $("#header").css("background-color", "#222222").css("box-shadow", "0 1px 3px 0px rgba(0, 0, 0, 0.1)");
        $(".headerOption").css("background", "#222222").css("border-left", "1px solid rgba(0, 0, 0, 0.2)");
    } else {
        $("#header").css("background-color", "initial").css("box-shadow", "initial");
        $(".headerOption").css("background", "initial").css("border-left", "initial");
    }
});

// Events
$(document).ready(init);

