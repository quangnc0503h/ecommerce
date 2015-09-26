'use strict';

angular.module('quangauthwebApp').controller('GroupCtrl', ['$scope', '$modal', '$location', 'groupService', 'localDataService',
    function ($scope, $modal, $location, groupService, localDataService) {
        var pagekey = 'GroupCtrlPageInfo';
        var pageInfo = { qsearch: '', parentId: '', currentPage: 1, itemsPerPage: 10 };
        if (localDataService.get(pagekey) !== null) {
            pageInfo = localDataService.get(pagekey);
        }
    // Load list function
        var loaded = false;
        $scope.items = {};
        $scope.qsearch = pageInfo.qsearch;
        $scope.ParentId = pageInfo.parentId;
        $scope.currentPage = pageInfo.currentPage;
        $scope.itemsPerPage = pageInfo.itemsPerPage;
        $scope.totalItems = 0;
        $scope.listAllParents = [];
        var loadList = function() {
            var filter = {
                Keyword: $scope.qsearch,
                ParentId: $scope.ParentId,
                PageSize: $scope.itemsPerPage,
                PageNumber: $scope.currentPage - 1,
                OrderBy: '',
            }
            groupService.listGroup(filter, function(res) {
                $scope.items = res.items;
                $scope.totalItems = res.totalCount;
                loaded = true;
                $scope.currentPage = pageInfo.currentPage;
            });
            loaded = false;
        };
    // Load list parent
        groupService.listAllOptions(function(items) {
            $scope.listAllParents = items;
        });

    // On ready should load list
        loadList();
        // Action when click button search
        $scope.searchItems = function() {
            $scope.currentPage = pageInfo.currentPage = 1;
            pageInfo.qsearch = $scope.qsearch;
            pageInfo.parentId = $scope.ParentId;
            localDataService.set(pagekey, pageInfo);
            loadList();
        };
        // Action when change page number
        $scope.pageChanged = function () {
            if (loaded) {
                pageInfo.currentPage = $scope.currentPage;
                localDataService.set(pagekey, pageInfo);
                loadList();
            }
        };

    // Action when click check all
        $scope.checkAll = function() {
            if ($scope.selectedAll) {
                $scope.selectedAll = true;
            } else {
                $scope.selectedAll = false;
            }
            angular.forEach($scope.items, function(item) {
                item.Selected = $scope.selectedAll;
            });
        };

    // Action when click button remove
        $scope.removeItem = function(itemId) {
            if (confirm("Bạn có chắc muốn xóa nhóm này?")) {
                $scope["isDisabledBtnDelete" + itemId] = true;
                groupService.removeGroup(itemId, function(success) {
                    if (success) {
                        // Reload list
                        loadList();

                        // Reload list parent
                        groupService.listAllOptions(function(items) {
                            $scope.listAllParents = items;
                        });
                    } else {
                        alert('Error: Pls try again!');
                    }
                    $scope["isDisabledBtnDelete" + itemId] = false;
                });
            }
        };

    // Action when click button remove all checked items
        $scope.removeCheckedItems = function() {
            var checkedItems = [];
            angular.forEach($scope.items, function(item) {
                if (item.Selected) {
                    checkedItems.push(item.Id);
                }
            });
            if (checkedItems.length > 0) {
                if (confirm("Bạn có chắc muốn xóa các nhóm đã chọn?")) {
                    $scope.isDisabledBtnDeleteAll = true;
                    groupService.removeListGroup(checkedItems, function(success) {
                        if (success) {
                            // Reload list
                            loadList();

                            // Reload list parent
                            groupService.listAllOptions(function(items) {
                                $scope.listAllParents = items;
                            });
                        } else {
                            alert('Error: Pls try again!');
                        }
                        $scope.isDisabledBtnDeleteAll = false;
                    });
                }
            } else {
                alert('Vui lòng chọn nhóm để xóa');
            }
        };
        // Modal create new item
        $scope.openDialogCreateItem = function () {

            var modalInstance = $modal.open({
                templateUrl: 'modalCreateGroupContent.html',
                controller: 'ModalCreateGroupCtrl',
                resolve: {
                    listAllParents: function () {
                        return $scope.listAllParents;
                    },
                    parentId: function () {
                        return $scope.ParentId ? parseInt($scope.ParentId) : '';
                    }
                }
            });

            modalInstance.result.then(function (success) {
                if (success) {
                    // Reload list
                    loadList();

                    // Reload list parent
                    groupService.listAllOptions(function (items) {
                        $scope.listAllParents = items;
                    });
                }
            }, function () {
                //$log.info('Modal dismissed at: ' + new Date());
            });
        };

        // Modal edit selected item
        $scope.openDialogEditItem = function (id) {

            var modalInstance = $modal.open({
                templateUrl: 'modalEditGroupContent.html',
                controller: 'ModalEditGroupCtrl',
                resolve: {
                    listAllParents: function () {
                        return $scope.listAllParents;
                    },
                    itemId: function () {
                        return id;
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
        };

        // Model grant group permission
        $scope.openDialogGrantUser = function (item) {

            var modalInstance = $modal.open({
                templateUrl: 'modalGrantGroupContent.html',
                controller: 'ModalGrantGroupCtrl',
                resolve: {
                    group: function () {
                        return item;
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
        };
    }]).controller('ModalCreateGroupCtrl', ['$scope', 'groupService', '$modalInstance', 'listAllParents', 'parentId', function ($scope, groupService, $modalInstance, listAllParents, parentId) {
        $scope.item = { Status: 1, ParentId: parentId };
        $scope.listAllParents = listAllParents;
        $scope.createItem = function () {
            console.log($scope.addForm.$valid);
            if ($scope.addForm.$valid) {
                $scope.isDisabledBtnCreate = true;
                groupService.createGroup($scope.item, function (success) {
                    if (success) {
                        $modalInstance.close(true);
                    } else {
                        alert('Error: Pls try again!');
                    }
                    $scope.isDisabledBtnCreate = false;
                });
            }
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('cancel');
        };
    }])
  .controller('ModalEditGroupCtrl', ['$scope', 'groupService', '$modalInstance', 'listAllParents', 'itemId', function ($scope, groupService, $modalInstance, listAllParents, itemId) {

      $scope.item = {};
      $scope.listAllParents = listAllParents;

      groupService.getGroup(itemId, function (item) {
          $scope.item = item;
      });

      $scope.updateItem = function () {
          if ($scope.editForm.$valid) {
              $scope.isDisabledBtnUpdate = true;
              groupService.saveGroup($scope.item, function (success) {
                  if (success) {
                      $modalInstance.close(true);
                  } else {
                      alert('Error: Pls try again!');
                  }
                  $scope.isDisabledBtnUpdate = false;
              });
          }
      };

      $scope.cancel = function () {
          $modalInstance.dismiss('cancel');
      };
  }]);