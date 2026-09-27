(function($) {
    $.validator.addMethod(
        "minimummarkup",
        function(value, element, params) {
            if (this.optional(element)) {
                return true;
            }

            const price =
                parseFloat(value);

            const percentage =
                parseFloat(
                    params.percent);

            const form =
                $(element)
                    .closest("form");

            const costInput =
                form.find(
                    "[name='" +
                    params.costfield +
                    "']");

            const cost =
                parseFloat(
                    costInput.val()) || 0;

            if (cost <= 0) {
                return true;
            }

            const minimumPrice =
                cost *
                (1 +
                    percentage / 100);

            return price >=
                minimumPrice;
        });

    $.validator.unobtrusive
        .adapters.add(
            "minimummarkup",
            [
                "percent",
                "costfield"
            ],
            function(options) {
                options.rules[
                    "minimummarkup"
                ] = {
                    percent:
                        options.params.percent,
                    costfield:
                        options.params.costfield
                };

                options.messages[
                    "minimummarkup"
                ] =
                    options.message;
            });
})(jQuery);