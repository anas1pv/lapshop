var ClsItems = {
    currentSearch: "",
    currentCategoryId: 0,
    GetAll: function (search, categoryId) {
        ClsItems.currentSearch = search || "";
        ClsItems.currentCategoryId = parseInt(categoryId) || 0;

        // Initialize Pagination using AJAX dataSource
        ClsItems.InitPagination();

        // Attach event handlers to checkboxes to trigger refresh
        $('.filter-checkbox').on('change', function() {
            ClsItems.RefreshPagination();
        });
    },
    InitPagination: function() {
        $('#ItemPagination').pagination({
            dataSource: '/api/Items/GetPaged',
            locator: 'data.items',
            totalNumberLocator: function(response) {
                if (response && response.data) {
                    return response.data.totalCount;
                }
                return 0;
            },
            pageSize: 24,
            alias: {
                pageNumber: 'page',
                pageSize: 'pageSize'
            },
            ajax: {
                data: function() {
                    // Collect active filters to send with AJAX
                    var selectedBrands = [];
                    $('.brand-filter:checked').each(function() {
                        selectedBrands.push($(this).data('val'));
                    });

                    var selectedRam = [];
                    $('.ram-filter:checked').each(function() {
                        selectedRam.push($(this).data('val'));
                    });

                    return {
                        categoryId: ClsItems.currentCategoryId,
                        search: ClsItems.currentSearch,
                        brands: selectedBrands.join(','),
                        ramSizes: selectedRam.join(',')
                    };
                },
                beforeSend: function() {
                    $('#ItemArea').html('<div class="col-12 text-center" style="padding: 80px 0;"><div class="spinner-border text-primary" role="status" style="border-color: #7C3AED; border-right-color: transparent;"><span class="sr-only">Loading...</span></div></div>');
                }
            },
            callback: function (data, pagination) {
                // Update showing text
                var pageNum = pagination.pageNumber;
                var pageSize = pagination.pageSize;
                var totalCount = pagination.totalNumber;
                var start = totalCount > 0 ? (pageNum - 1) * pageSize + 1 : 0;
                var end = Math.min(pageNum * pageSize, totalCount);
                $('.search-count h5').text("Showing Products " + start + "-" + end + " of " + totalCount + " Results");

                var htmlData = "";
                if (data && data.length > 0) {
                    for (var i = 0; i < data.length; i++) {
                        htmlData += ClsItems.DrawItem(data[i]);
                    }
                } else {
                    htmlData = '<div class="col-12 text-center" style="padding: 80px 20px;"><i class="ti-search" style="font-size: 48px; color: #64748B; display:block; margin-bottom:15px;"></i><h5 style="color:#fff;">No laptops found matching your filters</h5></div>';
                }
                
                var d1 = document.getElementById('ItemArea');
                if (d1) {
                    d1.innerHTML = htmlData;
                }
            }
        });
    },
    RefreshPagination: function() {
        ClsItems.InitPagination();
    },
    DrawItem: function (item) {
        var data = "<div class='col-xl-3 col-6 col-grid-box'>";
        data += "<div class='product-box'><div class='img-wrapper'>";
        var img = item.imageName || 'silver_ultrabook.png';
        data += "<div class='front'> <a href='/Items/ItemDetails/" + item.itemId + "'><img src='/Uploads/Items/" + img + "' class='img-fluid blur-up lazyload bg-img' alt=''></a></div>";
        data += "<div class='cart-info cart-wrap'>";
        data += "<a href='javascript:void(0)' onclick='ClsItems.AddToCart(" + item.itemId + ")' title='Add to cart'><i class='ti-shopping-cart'></i></a>";
        data += "<a href='/Items/ItemDetails/" + item.itemId + "' title='View details'><i class='ti-search' aria-hidden='true'></i></a>";
        data += "</div></div>";
        data += "<div class='product-detail'><div class='rating'> <i class='fa fa-star'></i> <i class='fa fa-star'></i> <i class='fa fa-star'></i>";
        data += "<i class='fa fa-star'></i> <i class='fa fa-star'></i></div>";
        data += "<a href='/Items/ItemDetails/" + item.itemId + "'><h6>" + item.itemName + "</h6></a> <p style='font-size:12px; color:#64748B; margin: 4px 0;'>" + (item.processor || '') + " | " + (item.ramSize || 8) + "GB RAM</p>";
        data += "<h4>$" + item.salesPrice + "</h4>";
        data += "</div> </div> </div>";
        return data;
    },
    AddToCart: function(itemId) {
        Helper.AjaxCallGet("/Order/AddToCartAjax?itemId=" + itemId, {}, "json", function(response) {
            if (response && response.success) {
                // Update badge in header
                var badge = $('#cartCountBadge');
                if (badge.length) {
                    badge.text(response.cartCount);
                    badge.show();
                }
                NotificationHelper.ShowSuccess("Laptop added to cart successfully!");
            } else {
                NotificationHelper.ShowError("Could not add item to cart.");
            }
        }, function() {
            NotificationHelper.ShowError("Connection error while adding to cart.");
        });
    }
}
