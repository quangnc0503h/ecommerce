'use strict';

/**
 * @ngdoc function
 * @name authclientApp.controller:PermissionCtrl
 * @description
 * # PermissionCtrl
 * Controller of the authclientApp
 */
angular.module('authclientApp')
  .controller('PermissionCtrl', ['$scope', '$modal', '$location', 'permissionService', 'localDataService', function ($scope, $modal, $location, permissionService, localDataService) {
      var pagekey = 'PermissionCtrlPageInfo';
      var pageInfo = { qsearch: '', currentPage: 1, itemsPerPage: 10 };
      if (localDataService.get(pagekey) !== null) {
          pageInfo = localDataService.get(pagekey);
      }
      $scope.items = {};
      $scope.qsearch = pageInfo.qsearch;
      $scope.currentPage = pageInfo.currentPage;
      $scope.itemsPerPage = pageInfo.itemsPerPage;
      $scope.totalItems = 0;
      
      // Load list function
      var loaded = false;
      var loadList = function () {
          var filter = {
              Keyword: $scope.qsearch,
              ParentId: $scope.ParentId,
              PageSize: $scope.itemsPerPage,
              PageNumber: $scope.currentPage - 1,
              OrderBy: '',
          }
          permissionService.listPermission(filter, function (res) {
              $scope.items = res.items;
              $scope.totalItems = res.totalCount;
              loaded = true;
              $scope.currentPage = pageInfo.currentPage;
          });
          loaded = false;
      };

      // On ready should load list
      loadList();

      // Action when click button search
      $scope.searchItems = function () {
          $scope.currentPage = pageInfo.currentPage = 1;
          pageInfo.qsearch = $scope.qsearch;
          localDataService.set(pagekey, pageInfo);
          loadList();
      }

      // Action when change page number
      $scope.pageChanged = function () {
          if (loaded) {
              pageInfo.currentPage = $scope.currentPage;
              localDataService.set(pagekey, pageInfo);
              loadList();
          }
      };

      // Action when click check all
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

      // Action when click button remove
      $scope.removeItem = function (itemId) {
          if (confirm("Bạn có chắc muốn xóa quyền này?")) {
              $scope["isDisabledBtnDelete" + itemId] = true;
              permissionService.removePermission(itemId, function (success) {
                  if (success) {
                      // Reload list
                      loadList();
                  } else {
                      alert('Error: Pls try again!');
                  }
                  $scope["isDisabledBtnDelete" + itemId] = false;
              });
          }
      };

      // Action when click button remove all checked items
      $scope.removeCheckedItems = function () {
          var checkedItems = [];
          angular.forEach($scope.items, function (item) {
              if (item.Selected) {
                  checkedItems.push(item.Id);
              }
          });
          if (checkedItems.length > 0) {
              if (confirm("Bạn có chắc muốn xóa các quyền đã chọn?")) {
                  $scope.isDisabledBtnDeleteAll = true;
                  permissionService.removeListPermission(checkedItems, function (success) {
                      if (success) {
                          // Reload list
                          loadList();
                      } else {
                          alert('Error: Pls try again!');
                      }
                      $scope.isDisabledBtnDeleteAll = false;
                  });
              }
          } else {
              alert('Vui lòng chọn quyền để xóa');
          }
      }

      // Link to grant permission page
      $scope.goToGrantItem = function (id) {
          $location.path('/role/grant/' + id);
      }

      // Modal create new item
      $scope.openDialogCreateItem = function () {

          var modalInstance = $modal.open({
              templateUrl: 'modalCreatePermissionContent.html',
              controller: 'ModalCreatePermissionCtrl',
              resolve: {
              }
          });

          modalInstance.result.then(function (success) {
              if (success) {
                  // Reload list
                  loadList();
              }
          }, function () {
              //$log.info('Modal dismissed at: ' + new Date());
          });
      };

      // Modal edit selected item
      $scope.openDialogEditItem = function (id) {

          var modalInstance = $modal.open({
              templateUrl: 'modalEditPermissionContent.html',
              controller: 'ModalEditPermissionCtrl',
              resolve: {
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
  }])
.controller('ModalCreatePermissionCtrl', ['$scope', 'permissionService', '$modalInstance', function ($scope, permissionService, $modalInstance) {
    $scope.item = { Status: 1 };
    $scope.createItem = function () {
        if ($scope.addForm.$valid) {
            $scope.isDisabledBtnCreate = true;
            permissionService.createPermission($scope.item, function (success) {
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
  .controller('ModalEditPermissionCtrl', ['$scope', 'permissionService', '$modalInstance', 'itemId', function ($scope, permissionService, $modalInstance, itemId) {
      $scope.item = {};

      permissionService.getPermission(itemId, function (item) {
          $scope.item = item;
      });

      $scope.updateItem = function () {
          if ($scope.editForm.$valid) {
              $scope.isDisabledBtnUpdate = true;
              permissionService.savePermission($scope.item, function (success) {
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
  }])
.controller('PermissionGrantCtrl', ['$scope', '$location', '$routeParams', 'userService', 'permissionService', 'termService', function ($scope, $location, $routeParams, userService, permissionService, termService) {
    $scope.allowItems = [];
    $scope.denyItems = [];
    $scope.listRoleOptions = {};
    $scope.permission = {};
    $scope.listRoles = [];

    // Get current permission
    permissionService.getPermission($routeParams.id, function (permission) {
        $scope.permission = permission;
    });

    // Load list role options
    termService.listRoleOptions(function (items) {
        $scope.listRoleOptions = items;
        $scope.listRoles = [];
        angular.forEach(items, function (value, key) {
            $scope.listRoles.push(value.RoleKeyLabel);
        });
    });

    // Get current permission grants
    permissionService.getPermissionGrants($routeParams.id, function (res) {
        $scope.allowItems = res.allowItems;
        $scope.denyItems = res.denyItems;
    });
    
    //$scope.isDisabledBtnUpdate = false;
    // Action when click update grant
    $scope.updatePermissionGrant = function () {
        if (confirm("Bạn đã chắc chắn thực hiện?")) {
            $scope.isDisabledBtnUpdate = true;
            permissionService.updatePermissionGrants($routeParams.id, $scope.allowItems, $scope.denyItems, function (success) {
                if (success) {
                    alert("Đã thực hiện thiết lập thành công");
                } else {
                    alert('Error: Pls try again!');
                }
                $scope.isDisabledBtnUpdate = false;
            });
        }
    };

    $scope.addItemAllow = function () {
        $scope.allowItems.push({ IsExactPattern: false });
    }

    $scope.removeItemAllow = function (index) {
        var newItems = [];
        for (var i = 0; i < $scope.allowItems.length; i++) {
            if (i != index) {
                newItems.push($scope.allowItems[i]);
            }
        }
        $scope.allowItems = newItems;
    }

    $scope.addItemDeny = function () {
        $scope.denyItems.push({ IsExactPattern: false });
    }

    $scope.removeItemDeny = function (index) {
        var newItems = [];
        for (var i = 0; i < $scope.denyItems.length; i++) {
            if (i != index) {
                newItems.push($scope.denyItems[i]);
            }
        }
        $scope.denyItems = newItems;
    }

    // Action when click check access all
    $scope.checkIsAccessAll = function () {
        if ($scope.isAccessAll) {
            $scope.isAccessAll = true;
        } else {
            $scope.isAccessAll = false;
        }
        angular.forEach($scope.items, function (item) {
            item.IsAccess = $scope.isAccessAll;
        });
    };

    $scope.backToList = function () {
        $location.path('/role');
    }
}]);
