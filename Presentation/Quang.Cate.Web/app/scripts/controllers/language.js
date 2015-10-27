'use strict';
angular.module('quangcatewebApp')
  .controller('LanguageCtrl', ['$scope', '$modal', '$location', 'localDataService', 'languageService', function ($scope, $modal, $location, localDataService, languageService) {
      var pagekey = 'LanguageCtrlPageInfo';
      var pageInfo = { qsearch: "", currentPage: 1, itemsPerPage: 20 };
      if (localDataService.get(pagekey) !== null) {
          pageInfo = localDataService.get(pagekey);
      }
      $scope.cultures = [];
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
            console.log(res.items);
        });
        loaded = false;
    }
      // Load list parent
    languageService.listAllCultures(function (items) {
        $scope.cultures = items;
       // console.log(items);
    });
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
    $scope.openDialogCreateItem = function (item) {
        var modalInstance = $modal.open({
            templateUrl: "modalCreateLanguageContent.html",
            controller: "ModalCreateLanguageCtrl",
            resolve: {
                cultures: function () {
                    return $scope.cultures;
                }
            }
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
            templateUrl: "modalEditLanguageContent.html",
            controller: "ModalEditLanguageCtrl",
            resolve: {  cultures: function () {
                return $scope.cultures;
            },
             itemId: function () { return id; }
            }
        });
        modalInstance.result.then(function (success) {
            if (success) {
                loadList();
            }
        }, function () {
            //$log.info('Modal dismissed at: ' + new Date());
        });
    }
  }]).controller("ModalCreateLanguageCtrl", ["$scope", "languageService", "$modalInstance", 'cultures', function ($scope, languageService, $modalInstance, cultures) {
      $scope.item = { Published: 1 };
      $scope.cultures = cultures;
      $scope.createItem = function () {
          if ($scope.addForm.$valid) {
              $scope.isDisabledBtnCreate = true;
              languageService.createLanguage($scope.item, function (success) {
                  if (success) {
                      $modalInstance.close(true);
                  } else {
                      alert("Error: Pls try again!");
                  }
                  $scope.isDisabledBtnCreate = false;
              });
          }

      };
      $scope.cancel = function () {
          $modalInstance.dismiss("cancel");
      }
  }]).controller('ModalEditLanguageCtrl', ['$scope', 'languageService', '$modalInstance', 'cultures', 'itemId', function ($scope, languageService, $modalInstance, cultures, itemId) {

      $scope.item = {};
      $scope.cultures = cultures;

      languageService.getLanguage(itemId, function (item) {
         // console.log(item);
          $scope.item = item;
      });

      $scope.updateItem = function () {
          if ($scope.editForm.$valid) {
              languageService.saveLanguage($scope.item, function (success) {
                  if (success) {
                      $modalInstance.close(true);
                  } else {
                      alert('Error: Pls try again!');
                  }
              });
          }
      };

      $scope.cancel = function () {
          $modalInstance.dismiss('cancel');
      };
  }]);
