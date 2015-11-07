'use strict';
angular.module("quangauthwebApp").controller("LoginHistoryCtrl", ["$scope", "$modal", "$location", "deviceService", "loginHistoryService", "localDataService", function ($scope, $modal, $location, deviceService, loginHistoryService, localDataService) {
    var pagekey = "pLoginHistoryCtrlPageInfo",
        pageInfo = { qsearch: { ThoiGianBatDau: new Date(2e3, 0, 1, 0, 0, 0, 0), ThoiGianKetThuc: new Date(2e3, 0, 1, 23, 59, 59, 0) }, currentPage: 1, itemsPerPage: 20 };
    if (localDataService.get(pagekey) !== null) {
        pageInfo = localDataService.get(pagekey);
        pageInfo.qsearch.NgayBatDau = pageInfo.qsearch.NgayBatDau ? new Date(pageInfo.qsearch.NgayBatDau) : null;
        pageInfo.qsearch.NgayKetThuc = pageInfo.qsearch.NgayKetThuc ? new Date(pageInfo.qsearch.NgayKetThuc) : null,
         pageInfo.qsearch.ThoiGianBatDau = pageInfo.qsearch.ThoiGianBatDau ? new Date(pageInfo.qsearch.ThoiGianBatDau) : new Date(2e3, 0, 1, 0, 0, 0, 0),
         pageInfo.qsearch.ThoiGianKetThuc = pageInfo.qsearch.ThoiGianBatDau ? new Date(pageInfo.qsearch.ThoiGianKetThuc) : new Date(2e3, 0, 1, 23, 59, 59, 0);
    }
    
    $scope.items = {};
    $scope.qsearch = pageInfo.qsearch;
    $scope.currentPage = pageInfo.currentPage;
    $scope.itemsPerPage = pageInfo.itemsPerPage;
    $scope.totalItems = 0;
    $scope.maxSize = 8;
    $scope.missingLoginHistorys = [];
    var isChecked = function (a, b) {
        if (b && b.length > 0)
            for (var c = 0; c < b.length; c++)
                if (b[c] && b[c].ticked && b[c].key == a)
                    return true;
        return false;
    };
    $scope.allLoginTypes = [
        { ticked: isChecked(1, $scope.qsearch.Type), key: 1, value: "Form" },
        { ticked: isChecked(2, $scope.qsearch.Type), key: 2, value: "Device" },
        { ticked: isChecked(3, $scope.qsearch.Type), key: 3, value: "Api key" },
        { ticked: isChecked(4, $scope.qsearch.Type), key: 4, value: "Refresh token" },
        { ticked: isChecked(4, $scope.qsearch.Type), key: 5, value: "Change password" }
        ];
    $scope.allLoginStatuses = [
        { ticked: isChecked(1, $scope.qsearch.LoginStatus), key: 0, value: "BadRequest" },
        { ticked: isChecked(1, $scope.qsearch.LoginStatus), key: 1, value: "Success" },
        { ticked: isChecked(2, $scope.qsearch.LoginStatus), key: 2, value: "IncorrectInput" },
        { ticked: isChecked(3, $scope.qsearch.LoginStatus), key: 3, value: "InvalidDeviceKey" },
        { ticked: isChecked(4, $scope.qsearch.LoginStatus), key: 4, value: "InvalidToken" },
        { ticked: isChecked(5, $scope.qsearch.LoginStatus), key: 5, value: "InvalidHeader" },
        { ticked: isChecked(6, $scope.qsearch.LoginStatus), key: 6, value: "InvalidApiKey" },
        { ticked: isChecked(7, $scope.qsearch.LoginStatus), key: 7, value: "InvalidRefreshToken" },
        { ticked: isChecked(8, $scope.qsearch.LoginStatus), key: 8, value: "ErrorRefreshToken" },
        { ticked: isChecked(8, $scope.qsearch.LoginStatus), key: 9, value: "InvalidOldPassword" },
        { ticked: isChecked(8, $scope.qsearch.LoginStatus), key: 10, value: "InvalidCientId" },
        { ticked: isChecked(8, $scope.qsearch.LoginStatus), key: 11, value: "InvalidCientSecret" },
        { ticked: isChecked(8, $scope.qsearch.LoginStatus), key: 12, value: "InvalidCientActive" }];
    $scope.allApps = [];
    deviceService.listAllClients(function (b) {
        $scope.allApps = [];
        $scope.allApps.push({ ticked: isChecked(null, $scope.qsearch.AppId), key: null, value: "NULL" });
        for (var c = 0; c < b.length; c++)
            $scope.allApps.push({ ticked: isChecked(b[c].Id, $scope.qsearch.AppId), key: b[c].Id, value: b[c].Name })
    });
    $scope.openNgayBatDau = function ($event) {
        $event.preventDefault();
        $event.stopPropagation(),
        $scope.openedNgayBatDau = true;
    };
    $scope.openNgayKetThuc = function ($event) {
        $event.preventDefault(),
        $event.stopPropagation(),
        $scope.openedNgayKetThuc = true;
    };
    $scope.getLoginStatus = function (b) {
        for (var c = 0; c < $scope.allLoginStatuses.length; c++)
            if ($scope.allLoginStatuses[c].key == b)
                return $scope.allLoginStatuses[c].value
    };
    $scope.getLoginType = function (b) {
        for (var c = 0; c < $scope.allLoginTypes.length; c++)
            if ($scope.allLoginTypes[c].key == b)
                return $scope.allLoginTypes[c].value
    };
    $scope.getAppName = function (b) {
        for (var c = 0; c < $scope.allApps.length; c++)
            if (a.allApps[c].key == b)
                return $scope.allApps[c].value
    };
    var getMultiSelectValues = function (a) {
        if (a && a.length > 0) { for (var b = [], c = 0; c < a.length; c++) a[c].ticked && b.push(a[c].key); return b } return null
    };
    var loaded = false;
    var loadList = function () {
        var filter = {
            Type: getMultiSelectValues($scope.qsearch.Type),
            LoginStatus: getMultiSelectValues($scope.qsearch.LoginStatus),
            AppId: getMultiSelectValues($scope.qsearch.AppId),
            LoginTimeFrom: null,
            LoginTimeTo: null,
            UserName: $scope.qsearch.UserName,
            ClientIP: $scope.qsearch.ClientIP,
            ClientApiKey: $scope.qsearch.ClientApiKey,
            ClientDevice: $scope.qsearch.ClientDevice,
            ClientUri: $scope.qsearch.ClientUri,
            RefreshToken: $scope.qsearch.RefreshToken,
            ClientUA: $scope.qsearch.ClientUA,
            PageSize: $scope.itemsPerPage,
            PageNumber: $scope.currentPage - 1, OrderBy: ""
        };
        $scope.qsearch.NgayBatDau && (filter.LoginTimeFrom = $scope.qsearch.NgayBatDau, filter.LoginTimeFrom.setHours($scope.qsearch.ThoiGianBatDau.getHours()), filter.LoginTimeFrom.setMinutes($scope.qsearch.ThoiGianBatDau.getMinutes()), filter.LoginTimeFrom.setSeconds(0), filter.LoginTimeFrom.setMilliseconds(0));
        $scope.qsearch.NgayKetThuc && (filter.LoginTimeTo = $scope.qsearch.NgayKetThuc, filter.LoginTimeTo.setHours($scope.qsearch.ThoiGianKetThuc.getHours()), filter.LoginTimeTo.setMinutes($scope.qsearch.ThoiGianKetThuc.getMinutes()), filter.LoginTimeTo.setSeconds(59), filter.LoginTimeTo.setMilliseconds(999));
        loginHistoryService.listLoginHistory(filter, function (res) {
            $scope.items = res.items;
            $scope.totalItems = res.totalCount;
            $scope.currentPage = pageInfo.currentPage;
            loaded = true;
            $scope.isDisabledBtnSearch = false;
        });
        loaded = false;
        $scope.isDisabledBtnSearch = true;
    };
    loadList();
    $scope.searchItems = function () {
        $scope.currentPage = pageInfo.currentPage = 1;
        pageInfo.qsearch = $scope.qsearch;
        localDataService.set(pagekey, pageInfo);
        loadList();
    };
    $scope.resetSearch = function () {
        $scope.qsearch = {
            Type: [],
            LoginStatus: [],
            NgayBatDau: null,
            NgayKetThuc: null,
            ThoiGianBatDau: new Date(2e3, 0, 1, 0, 0, 0, 0),
            ThoiGianKetThuc: new Date(2e3, 0, 1, 23, 59, 59, 0),
            AppId: [],
            UserName: null,
            ClientIP: null,
            ClientApiKey: null,
            ClientDevice: null,
            ClientUri: null
        };
        for (var b = 0; b < $scope.allLoginTypes.length; b++)
            $scope.allLoginTypes[b].ticked = !1;
        for (var b = 0; b < $scope.allLoginStatuses.length; b++)
            $scope.allLoginStatuses[b].ticked = !1;
        for (var b = 0; b < $scope.allApps.length; b++)
            $scope.allApps[b].ticked = !1; a.searchItems()
    };
    $scope.pageChanged = function () {
        if (loaded) {
            pageInfo.currentPage = $scope.currentPage;
            localDataService.set(pagekey, pageInfo);
            loadList();
        }
    };
    $scope.checkAll = function () {
        if ($scope.selectedAll) {
            $scope.selectedAll = true;
        } else {
            $scope.selectedAll = false;
        }
        angular.forEach($scope.items, function (item) {
            item.Selected = $scope.selectedAll;
        });
    };
    $scope.removeItem = function (itemId) {

        confirm("Bạn có chắc muốn xóa thiết bị này?") && ($scope["isDisabledBtnDelete" + itemId] = true, loginHistoryService.removeLoginHistory(itemId, function (success) {
            success ? (loadList()) : alert("Error: Pls try again!"),
                $scope["isDisabledBtnDelete" + itemId] = true
        }))
    };
    $scope.removeCheckedItems = function () {
        var checkedItems = [];
        angular.forEach($scope.items, function (item) {
            item.Selected && checkedItems.push(item.Id)
        });
        if (checkedItems.length > 0)
        {
            if (confirm("Bạn có chắc muốn xóa các thiết bị đã chọn?")) {
                $scope.isDisabledBtnDeleteAll = true;
                loginHistoryService.removeListLoginHistory(checkedItems, function (success) {
                    if (success) {
                        loadList();
                        //reloadMissingDevices();
                    } else {
                        alert("Error: Pls try again!");
                    }
                    $scope.isDisabledBtnDeleteAll = false;
                });
            }
        }
        else {
            alert("Vui lòng chọn thiết bị để xóa");
        }        
    };
    $scope.openDialogViewItem = function (c) {
        var modalInstance = $modal.open({
            templateUrl: "modalViewLoginHistoryContent.html",
            controller: "ModalViewLoginHistoryCtrl",
            resolve: {
                itemId: function () { return c },
                getLoginType: function () { return $scope.getLoginType },
                getLoginStatus: function () { return $scope.getLoginStatus },
                getAppName: function () { return $scope.getAppName }
            }
        });
        modalInstance.result.then(function (success) {
            if (success) {
                loadList();
            }
        }, function () { })
    }
}]).controller("ModalViewLoginHistoryCtrl", ["$scope", "loginHistoryService", "$modalInstance", "itemId", "getLoginType", "getLoginStatus", "getAppName", function ($scope, loginHistoryService, $modalInstance, itemId, getLoginType, getLoginStatus, getAppName) {
    $scope.item = {};
    $scope.getLoginType = getLoginType;
    $scope.getLoginStatus = getLoginStatus;
    $scope.getAppName = getAppName;
    loginHistoryService.getLoginHistory(itemId, function (item) {
        $scope.item = item;
    });
    $scope.cancel = function () {
        $modalInstance.dismiss("cancel");
    }
}]);