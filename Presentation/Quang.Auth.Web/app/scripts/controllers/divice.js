'use strict';
angular.module('quangauthwebApp').controller('DeviceCtrl', [
    '$scope', '$modal', '$location', 'deviceService', 'localDataService',
    function ($scope, $modal, $location, deviceService, localDataService) {
        var pagekey = 'DeviceCtrlPageInfo';
        var pageInfo = { qsearch: "", currentPage: 1, itemsPerPage: 20 };
        if (localDataService.get(pagekey) !== null) {
            pageInfo = localDataService.get(pagekey);
        }
        var loaded = false;
        $scope.items = {},
        $scope.qsearch = pageInfo.qsearch,
        $scope.currentPage = pageInfo.currentPage,
        $scope.itemsPerPage = pageInfo.itemsPerPage,
        $scope.totalItems = 0,
        $scope.missingDevices = [];
        var loadList = function() {
            var filter = {
                Keyword: $scope.qsearch,
                PageSize: $scope.itemsPerPage,
                PageNumber: $scope.currentPage - 1,
                OrderBy: ""
            }
            deviceService.listDevice(filter, function (res) {
                $scope.items = res.items,
                $scope.totalItems = res.totalCount,
                loaded = true,
                $scope.currentPage = pageInfo.currentPage;
            });
            loaded = false;
        }
        loadList();

        $scope.searchItems = function () {
            $scope.currentPage = pageInfo.currentPage = 1,
            pageInfo.qsearch = $scope.qsearch,
            localDataService.set(pagekey, pageInfo);
            loadList();
        }
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
        }
        $scope.removeItem = function (itemId) {
            if (confirm("Bạn có chắc muốn xóa thiết bị này?")){
                $scope["isDisabledBtnDelete" + itemId] = true;
                deviceService.removeDevice(itemId, function (success) {
                    if (success) {
                        loadList();
                        reloadMissingDevices();
                    } else {
                        alert('Error: Pls try again!');
                    }
                    $scope["isDisabledBtnDelete" + itemId] = false;                    
                })
            }
                 
        }

        $scope.removeCheckedItems = function () {
            var checkedItems = [];
            angular.forEach($scope.items, function(item) {
                if (item.Selected) {
                    checkedItems.push(item.Id);
                }
            });
            if (checkedItems.length > 0) {
                if (confirm("Bạn có chắc muốn xóa các thiết bị đã chọn?")) {
                    $scope.isDisabledBtnDeleteAll = true;
                    deviceService.removeListDevice(checkedItems, function (success) {
                        if (success) {
                            loadList();
                            reloadMissingDevices();
                        } else {
                            alert("Error: Pls try again!");
                        }
                        $scope.isDisabledBtnDeleteAll = false;                        
                    });
                }
            }
            else {
                alert("Vui lòng chọn thiết bị để xóa")
            }
            checkedItems.length > 0 ? confirm("Bạn có chắc muốn xóa các thiết bị đã chọn?") && ($scope.isDisabledBtnDeleteAll = true, deviceService.removeListDevice(b, function (b) { b ? (i(), reloadMissingDevices()) : alert("Error: Pls try again!"), a.isDisabledBtnDeleteAll = !1 })) : alert("Vui lòng chọn thiết bị để xóa")
        }
        $scope.openDialogRequestWaiting = function () {
            var modalInstance = $modal.open({
                templateUrl: "modalRequestDeviceContent.html",
                controller: "ModalRequestDeviceCtrl",
                size: "lg", resolve: {}
            });
            modalInstance.result.then(function (item) {
                //b && b.DeviceKey;
                deviceService.getDeviceByKey(item.DeviceKey, function (res) {
                    res ? $scope.openDialogEditItem(res.Id) : $scope.openDialogCreateItem(item)
                })
            }, function () { });
        }
        $scope.openDialogCreateItem = function (item) {
            var modalInstance = $modal.open({
                templateUrl: "modalCreateDeviceContent.html",
                controller: "ModalCreateDeviceCtrl",
                resolve: { device: function () { return item } }
            });
            modalInstance.result.then(function (success) {
                if (success) {
                    loadList();
                }
            }, function () {
                //$log.info('Modal dismissed at: ' + new Date());
            });
        }
        $scope.openDialogEditItem = function (id) {
            var modalInstance = $modal.open({
                templateUrl: "modalEditDeviceContent.html",
                controller: "ModalEditDeviceCtrl",
                resolve: { itemId: function () { return id } }
            });
            modalInstance.result.then(function (success) {
                if (success) {
                    loadList();
                }
            }, function () {
                //$log.info('Modal dismissed at: ' + new Date());
            });
        }
    }
]).controller("ModalRequestDeviceCtrl", ["$scope", "$filter", "$modalInstance", "localDataService", "deviceService", function ($scope, $filter, $modalInstance, localDataService, deviceService) {
    var pagekey = "RequestDeviceCtrlPageInfo";
    var pageInfo = { qsearch: "", qdatefrom: "", qdateto: "", currentPage: 1, itemsPerPage: 20 };
    if (localDataService.get(pagekey) !== null) {
        pageInfo = localDataService.get(pagekey);
    }
    $scope.items = {};
    $scope.qsearch = pageInfo.qsearch;
    $scope.qdatefrom = pageInfo.qdatefrom;
    $scope.qdateto = pageInfo.qdateto;
    $scope.currentPage = pageInfo.currentPage;
    $scope.itemsPerPage = pageInfo.itemsPerPage;
    $scope.totalItems = 0;
    var loaded = false;
    var loadList = function () {
        var filter = {
            Keyword: $scope.qsearch,
            DateFrom: $scope.qdatefrom ? $filter("date")($scope.qdatefrom, "yyyy-MM-dd") : null,
            DateTo: $scope.qdateto ? $filter("date")($scope.qdateto, "yyyy-MM-dd") : null,
            PageSize: $scope.itemsPerPage,
            PageNumber: $scope.currentPage - 1,
            OrderBy: ""
        };
        deviceService.listRequestDevice($filter, function (res) {
            $scope.items = res.items;
            $scope.totalItems = res.totalCount;
            loaded = true;
            $scope.currentPage = pageInfo.currentPage;
        });
        loaded = false;
    }; loadList(),
    $scope.datePickerOpenedFrom = false;
    $scope.openDatePickerFrom = function (event) {
        event.preventDefault();
        event.stopPropagation();
        $scope.datePickerOpenedFrom = !$scope.datePickerOpenedFrom
    };
    $scope.datePickerOpenedTo = false;
    $scope.openDatePickerTo = function (event) {
        event.preventDefault();
        event.stopPropagation();
        $scope.datePickerOpenedTo = !$scope.datePickerOpenedTo
    };
    $scope.searchItems = function () {
        $scope.currentPage = pageInfo.currentPage = 1;
        pageInfo.qsearch = $scope.qsearch;
        pageInfo.qdatefrom = $scope.qdatefrom;
        pageInfo.qdateto = $scope.qdateto;
        localDataService.set(pagekey, pageInfo);
        loadList()
    }
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
        if (confirm("Bạn có chắc muốn xóa yêu cầu này?")) {
            $scope["isDisabledBtnDelete" + itemId] = true;
            deviceService.removeRequestDevice(itemId, function (success) {
                if (success) {
                    loadList();
                } else {
                    alert("Error: Pls try again!")
                }
                $scope["isDisabledBtnDelete" + itemId] = false;
            })
        }
        
    }
    $scope.removeCheckedItems = function () {
        var checkedItems = [];
        angular.forEach($scope.items, function(item) {
            if (item.Selected) {
                checkedItems.push(item.Id);
            }
        });
        if (checkedItems.length > 0) {
            if (confirm("Bạn có chắc muốn xóa các yêu cầu đã chọn?")) {
                $scope.isDisabledBtnDeleteAll = true;
                deviceService.removeListRequestDevice(checkedItems,
                    function (success) {
                        if (success) {
                            loadList();
                        } else {
                            alert("Error: Pls try again!");
                        }
                        $scope.isDisabledBtnDeleteAll = false;
                    });
            }
            else {
                alert("Vui lòng chọn yêu cầu để xóa");
            }
        }        
    }
    $scope.applyItem = function (item) {
        $modalInstance.close(item)
    };
    $scope.getTrustText = function (a) {
        return a ? "Có" : "Không"
    };
    $scope.close = function () {
        $modalInstance.dismiss("cancel")
    }
}]).controller("ModalCreateDeviceCtrl", ["$scope", "deviceService", "$modalInstance", "device", function ($scope, deviceService, $modalInstance, device) {
    $scope.item = { IsActived: 1 };
    if (device != undefined) {
        $scope.item.RequestDeviceId = device.Id;
        $scope.item.RequestDeviceName = device.RequestName;
        $scope.item.DeviceKey = device.DeviceKey;
        $scope.item.SerialNumber = device.SerialNumber;
        $scope.item.IMEI = device.IMEI;
        $scope.item.Manufacturer = device.Manufacturer;
        $scope.item.Model = device.Model;
        $scope.item.Platform = device.Platform;
        $scope.item.PlatformVersion = device.PlatformVersion;
    }
    $scope.item.DeviceSecret = "";
    for (var e = 1; 6 >= e; e++) {
        var f = new String(Math.random());
        $scope.item.DeviceSecret += f.substr(f.length - 1);
    }
    $scope.createItem = function () {
        if ($scope.addForm.$valid) {
            $scope.isDisabledBtnCreate = true;
            b.createDevice($scope.item, function (success) {
                if (success) {
                    $modalInstance.close(true);
                } else {
                    alert("Error: Pls try again!");
                }                
                $scope.isDisabledBtnCreate = false;
            });
        }
        a.addForm.$valid && (a.isDisabledBtnCreate = !0, b.createDevice(a.item, function (b) { b ? c.close(!0) : alert("Error: Pls try again!"), a.isDisabledBtnCreate = !1 }))
    };
    $scope.cancel = function () {
        $modalInstance.dismiss("cancel")
    }
}]).controller("ModalEditDeviceCtrl", ["$scope", "deviceService", "$modalInstance", "itemId", function ($scope, deviceService, $modalInstance, itemId) {
    $scope.item = {};
    deviceService.getDevice(itemId, function (res) { $scope.item = res });
    $scope.updateItem = function () {
        if ($scope.editForm.$valid) {
            $scope.isDisabledBtnUpdate = true;
            deviceService.saveDevice($scope.item, function (success) {
                success ? $modalInstance.close(true) : alert("Error: Pls try again!");
                $scope.isDisabledBtnUpdate = false;
            });
        }        
    };
    $scope.cancel = function () {
        $modalInstance.dismiss("cancel");
    }
}]);