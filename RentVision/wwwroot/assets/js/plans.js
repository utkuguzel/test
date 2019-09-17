var plans = [];

function getUserPlans()
{
    $(".planFilter").on("change", onUserPlansFilter);

    $.ajax({
        method: "GET",
        url: "/api/getUserPlans",
        dataType: "json",
        success: onGetUserPlansSuccess,
        error: onGetUserPlansError
    });
}

function onUserPlansFilter()
{
    var payInterval = parseInt(this.value);
    var filteredPlans = getUserPlansByInterval(payInterval);

    if (filteredPlans !== undefined)
        updateUserPlans(payInterval);
}

function updateUserPlans(interval)
{
    var userPlans = $(".plan");

    // Get for each plan type that's displayed on the page their corresponding plan data (if it exists) and display it.
    $.each(userPlans, function (i, v)
    {
        var userPlanType = $(v).data("plan");

        if (userPlanType !== undefined)
        {
            var plan = getUserPlanByNameInterval(userPlanType, interval);

            if (plan !== undefined)
            {
                var price = (plan.price <= 0) ? "Free!" : "€ " + plan.price;

                $(v).find(".title").text(plan.name);
                $(v).find(".price").text(price);

                $(v).find(".button").attr("href", "/register?userPlan=" + plan.name.split(" ")[0] + "&payInterval=" + plan.payInterval);
            }
        }
    });
}

function onGetUserPlansSuccess(response)
{
    plans = response;

    // Default = 2 (Monthly)
    updateUserPlans(2);
}

function onGetUserPlansError(jqXhr, error, errorStr)
{
    console.log(error + ": " + errorStr);
}

function getUserPlanByNameInterval(name, interval)
{
    var planFound;

    $.each(plans, function (i, v)
    {
        var planName = v.name.split(" ")[0]; // Remove extra words such as 'Monthly' & 'Annually'

        if (planName === name && v.payInterval === interval)
        {
            planFound = v;
            return false; // Break out of the loop since we only expect 1 result
        }
    });

    return planFound;
}

function getUserPlansByInterval(interval)
{
    var plansFound = [];

    $.each(plans, function (i, v)
    {
        if (v.payInterval === interval)
            plansFound.push(v);
    });

    return plansFound;
}

$(document).ready(getUserPlans);