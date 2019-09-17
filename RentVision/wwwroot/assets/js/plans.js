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
    {
        updateUserPlans(payInterval);
    }
}

function updateUserPlans(interval)
{
    var userPlans = $(".plan");

    $.each(userPlans, function (i, v)
    {
        var userPlanType = $(v).data("plan");

        if (userPlanType !== undefined)
        {
            var plan = getUserPlanByNameInterval(userPlanType, interval);

            if (plan !== undefined)
            {
                var price = (plan.price <= 0) ? "Free" : plan.price;

                $(v).find(".title").text(plan.name);
                $(v).find(".price").text(price);
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
        if (v.name.split(" ")[0] === name && v.payInterval === interval)
        {
            planFound = v;
            return false;
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