function init()
{
    // Init header hover
    $(".headerHoverBar").slideUp();

    // This function resets certain elements to their default state (hidden) if another element that was not in their parent container was clicked
    // To prevent a messy header
    $(document).on("mousedown touchend", function (e)
    {
        if ($(e.target).closest("#rightmenu-mobile").length === 0 && $("#rightmenu-mobile").is(":visible") && $(e.target).closest("#rightmenu").length === 0 )
        {
            $("#rightmenu-mobile").find(".fas").removeClass("fa-times").addClass("fa-bars");
            $("#rightmenu").stop().fadeOut(150);
        }
    });

    // Fix for resizing client window, this will reset the mobile menu on resize
    $(window).resize(function ()
    {
        if ($(this).width() > 768)
        {
            $("#rightmenu").show();
        }
        else
        {
            $("#rightmenu").hide();
            $("#rightmenu-mobile").find(".fas").removeClass("fa-times").addClass("fa-bars");
            $("#rightmenu").stop().fadeOut(150);

            // Hide all visible dropdowns too
            $(".headerDropdown").hide();
        }
    });

    // This function handles hamburger menu clicks/touch
    $("#rightmenu-mobile").on("click", function (e)
    {
        //e.preventDefault();

        var icon = $(this).find(".fas");

        if (icon.hasClass("fa-bars"))
        {
            $(this).find(".fas").removeClass("fa-bars").addClass("fa-times");
            $("#rightmenu").stop().fadeIn(200);
            //console.log("FADE IN");
        }
        else
        {
            $(this).find(".fas").removeClass("fa-times").addClass("fa-bars");
            $("#rightmenu").stop().fadeOut(150);
            //console.log("FADE OUT");
        }
    });

    $(".headerOption").on("click", function (e)
    {
        var dropdown = $(this).find(".headerDropdown");

        if (dropdown && !dropdown.is(":visible"))
        {
            $(this).find("i").removeClass("fa-angle-down").addClass("fa-angle-up");
            dropdown.show();
        }
        else if (dropdown && dropdown.is(":visible"))
        {
            if ( !$(e.target).hasClass(".headerDropdown") )
            {
                $(this).find("i").removeClass("fa-angle-up").addClass("fa-angle-down");
                dropdown.hide();
            }
        }
    });

    $(".headerOption").on("mouseleave", function (e)
    {
        var dropdown = $(this).find(".headerDropdown");

        if (dropdown && $(window).width() > 768 )
        {
            $(this).find("i").removeClass("fa-angle-up").addClass("fa-angle-down");
            dropdown.hide();
        }
    });

    // Handle green hover bar above header option
    $(".headerOption, .block").on("mouseenter touchend", function (e)
    {
        $(this).find(".headerHoverBar").stop().slideDown(100);
    });

    // Handles green hover bar animation above header option
    $(".headerOption, .block").on("mouseleave touchend", function (e)
    {
        $(this).find(".headerHoverBar").stop().slideUp(100);
    });
}

function handleHeaderScroll(e)
{
	var top = $(document).scrollTop()

	$("#header").css("background-color", "rgba(255,255,255,1)").css("border-bottom", "1px solid #EEE");
	$("#header .headerOption").css("background-color", "rgba(255,255,255,1)").css("color", "#000000").css("border-bottom", "#EEE");

	if ( top <= 65 )
	{
		$("#header").css("background-color", "rgba(0,0,0,0.2)").css("border-bottom", "1px solid #222");
		$("#header .headerOption").css("background-color", "rgba(0,0,0,0.0)").css("color", "#FFFFFF").css("border-bottom", "rgba(255,255,255,0)");
	}
}

function hideLoadScreen()
{
    //$("#loading").stop().hide();
    $("#loading").stop().fadeOut(100);
}

// Events
$(document).ready(init);
//$(document).on("scroll", handleHeaderScroll);