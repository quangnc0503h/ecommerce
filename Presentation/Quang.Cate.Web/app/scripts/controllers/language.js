'use strict';
angular.module('quangcatewebApp')
  .controller('LanguageCtrl', ['$scope', '$modal', '$location', 'localDataService', 'languageService', function ($scope, $modal, $location, localDataService, languageService) {
      var pagekey = 'LanguageCtrlPageInfo';
      var pageInfo = { qsearch: "", currentPage: 1, itemsPerPage: 20 };
      if (localDataService.get(pagekey) !== null) {
          pageInfo = localDataService.get(pagekey);
      }
      var loaded = false;
    $scope.items = {},
        $scope.qsearch = pageInfo.qsearch,
        $scope.currentPage = pageInfo.currentPage,
        $scope.itemsPerPage = pageInfo.itemsPerPage,
        $scope.totalItems = 0;
    var loadList = function () {
        var filter = {
            Keyword: $scope.qsearch,
            PageSize: $scope.itemsPerPage,
            PageNumber: $scope.currentPage - 1,
            OrderBy: ""
        }
        languageService.listLanguage(filter, function (res) {
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
        if (confirm("Bạn có chắc muốn xóa ngôn ngữ này?")) {
            $scope["isDisabledBtnDelete" + itemId] = true;
            languageService.removeDevice(itemId, function (success) {
                if (success) {
                    loadList();
                    //reloadMissingDevices();
                } else {
                    alert('Error: Pls try again!');
                }
                $scope["isDisabledBtnDelete" + itemId] = false;
            });
        }

    }

    $scope.removeCheckedItems = function () {
        var checkedItems = [];
        angular.forEach($scope.items, function (item) {
            if (item.Selected) {
                checkedItems.push(item.Id);
            }
        });
        if (checkedItems.length > 0) {
            if (confirm("Bạn có chắc muốn xóa các ngôn ngữ đã chọn?")) {
                $scope.isDisabledBtnDeleteAll = true;
                languageService.removeListLanguage(checkedItems, function (success) {
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
            alert("Vui lòng chọn ngôn ngữ để xóa");
        }
        
    }
}]);
