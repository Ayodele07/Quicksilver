﻿var ProductPage = {
    init: function () {
        $(document).on('change', '.jsVariationSwitch', ProductPage.switchVariant);
    },
    switchVariant: function () {
        var form = $(this).closest('form');
        $.ajax({
            type: "POST",
            url: form[0].action,
            data: form.serialize(),
            success: function (result) {
                $('.jsProductDetails').replaceWith($(result).find('.jsProductDetails'));
            },
            error: function () {
                alert('The variant is out of stock.');                
        }
        });
    }
};