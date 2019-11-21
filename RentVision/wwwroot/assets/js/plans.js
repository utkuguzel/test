var plans = [];
var planFeatures = [];

function getUserPlans()
{
    $(".planFilter").on("change", onPlanFilter);

    $.ajax({
        method: "GET",
        url: "/api/plans/features",
        dataType: "json",
        success: onGetPlanFeatureSuccess,
        error: onGetPlanError
    });
}

function getPlanFeatures(planId) {
    var features = [];

    $.each(planFeatures, function (_, feature) {
        if (feature.planId === planId)
            features.push(feature);
    });

    return features;
}

function onPlanFilter()
{
    var payInterval = parseInt(this.value);
    var filteredPlans = getPlansByInterval(payInterval);

    if (filteredPlans !== undefined)
        updatePlans(payInterval);
}

function updatePlans(interval)
{
    var plans = $(".pricing-table-2");

    // Get for each plan type that's displayed on the page their corresponding plan data (if it exists) and display it.
    $.each(plans, function (i, v)
    {
        var planType = $(v).data("plan");

        if (planType !== undefined)
        {
            var plan = getPlanByNameInterval(planType, interval);

            if (plan !== undefined)
            {
                var ul = $(v).find(".price-body ul");
                var planFeatures = getPlanFeatures(plan.id);
                var price = (plan.price <= 0) ? "Free!" : plan.price;

                if (planType !== "Free") {
                    $(v).find(".title").text(plan.name);
                    $(v).find(".realPrice").text(price);
                }
                else {
                    var localizedTitle = $(v).data("plan-name-localized");

                    if (localizedTitle !== undefined) {
                        $(v).find(".title").text(localizedTitle);
                        $(v).find(".realPrice").text(localizedTitle);
                    }
                }

                //$(v).find(".order-btn").attr("href", "/register?userPlan=" + plan.name.split(" ")[0] + "&payInterval=" + plan.payInterval);
                $(v).find(".order-btn").attr("href", "/register/" + plan.id);

                ul.empty();
                $.each(planFeatures, function (_, feature) {
                    $(ul).append("<li>" + feature.description.replace("{0}", "<b>" + feature.value + "</b>") + "</li>");
                });
            }
        }
    });
}

function onGetPlanSuccess(response)
{
    plans = response;

    // Default = 2 (Monthly)
    updatePlans(2);
}

function onGetPlanError(_, error, errorStr)
{
    console.log(error + ": " + errorStr);
}

function onGetPlanFeatureSuccess(response) {
    planFeatures = response;

    $.ajax({
        method: "GET",
        url: "/api/plans",
        dataType: "json",
        success: onGetPlanSuccess,
        error: onGetPlanError
    });
}

function getPlanByNameInterval(name, interval)
{
    var planFound;

    $.each(plans, function (_, v)
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

function getPlansByInterval(interval)
{
    var plansFound = [];

    $.each(plans, function (_, v)
    {
        if (v.payInterval === interval)
            plansFound.push(v);
    });

    return plansFound;
}

$(document).ready(getUserPlans);