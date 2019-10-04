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

    // Error modal events
    $("#errorModal").on("mousedown touchend", function () {
        $(this).stop().fadeOut(250);
    });

    handleHeaderScroll(true);
}

function handleHeaderScroll(init) {
    var top = typeof init === Boolean ? 0 : this.scrollTop;

    if (top > 70) {
        $("#header").stop().animate({
            backgroundColor: "#222222",
            boxShadow: "0 1px 3px 0 rgba(0, 0, 0, 0.1)"
        }, 100);

        $(".headerOption").stop().animate({
            background: "#222222",
            borderLeft: "1px solid rgba(0, 0, 0, 0.2)"
        }, 100);

        $(".headerHoverBar.active").removeClass("active").closest(".headerOption").addClass("active");
        $(".headerOption").addClass("bg-visible");
    } else if (top <= 70) {
        $("#header").stop().animate({
            backgroundColor: "rgba(0, 0, 0, 0)",
            boxShadow: "initial"
        }, 100);

        $(".headerOption").stop().animate({
            background: "rgba(0, 0, 0, 0)",
            borderLeft: "initial"
        }, 100);

        $(".headerOption.active").removeClass("active").find(".headerHoverBar").addClass("active");
        $(".headerOption").removeClass("bg-visible");
    }
}

// Events
$(document).ready(init);
$("body").scroll(handleHeaderScroll);

