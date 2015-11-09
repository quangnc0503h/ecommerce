'use strict';

/**
 * @ngdoc function
 * @name authclientApp.controller:TermCtrl
 * @description
 * # TermCtrl
 * Controller of the authclientApp
 */
angular.module('authclientApp')
  .controller('TermCtrl', ['$scope', '$modal', '$location', 'termService', 'localDataService', function ($scope, $modal, $location, termService, localDataService) {
      var pagekey = 'TermCtrlPageInfo';
      var pageInfo = { qsearch: '', currentPage: 1, itemsPerPage: 20 };
      if (localDataService.get(pagekey) !== null) {
          pageInfo = localDataService.get(pagekey);
      }
      $scope.items = {};
      $scope.qsearch = pageInfo.qsearch;
      $scope.currentPage = pageInfo.currentPage;
      $scope.itemsPerPage = pageInfo.itemsPerPage;
      $scope.totalItems = 0;
      $scope.maxSize = 5;
      $scope.missingTerms = [];
      
      // Load list function
      var loaded = false;
      var loadList = function () {
          var filter = {
              Keyword: $scope.qsearch,
              PageSize: $scope.itemsPerPage,
              PageNumber: $scope.currentPage - 1,
              OrderBy: '',
          }
          termService.listTerm(filter, function (res) {
              $scope.items = res.items;
              $scope.totalItems = res.totalCount;
              loaded = true;
              $scope.currentPage = pageInfo.currentPage;
          });
          loaded = false;
      };

      // Load missing terms
      var reloadMissingTerms = function () {
          termService.getMissingTerms(function (missingTerms) {
              //console.log(missingTerms);
              $scope.missingTerms = missingTerms;
          });
      };
      reloadMissingTerms();

      // On ready should load list
      loadList();

      // Term when click button search
      $scope.searchItems = function () {
          $scope.currentPage = pageInfo.currentPage = 1;
          pageInfo.qsearch = $scope.qsearch;
          localDataService.set(pagekey, pageInfo);
          loadList();
      }

      // Term when change page number
      $scope.pageChanged = function () {
          if (loaded) {
              pageInfo.currentPage = $scope.currentPage;
              localDataService.set(pagekey, pageInfo);
              loadList();
          }
      };

      // Term when click check all
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

      // Term when click button remove
      $scope.removeItem = function (itemId) {
          if (confirm("Bạn có chắc muốn xóa chức năng này?")) {
              $scope["isDisabledBtnDelete" + itemId] = true;
              termService.removeTerm(itemId, function (success) {
                  if (success) {
                      // Reload list
                      loadList();

                      // Reload list missing terms
                      reloadMissingTerms();
                  } else {
                      alert('Error: Pls try again!');
                  }
                  $scope["isDisabledBtnDelete" + itemId] = false;
              });
          }
      };

      // Term when click button remove all checked items
      $scope.removeCheckedItems = function () {
          var checkedItems = [];
          angular.forEach($scope.items, function (item) {
              if (item.Selected) {
                  checkedItems.push(item.Id);
              }
          });
          if (checkedItems.length > 0) {
              if (confirm("Bạn có chắc muốn xóa các chức năng đã chọn?")) {
                  $scope.isDisabledBtnDeleteAll = true;
                  termService.removeListTerm(checkedItems, function (success) {
                      if (success) {
                          // Reload list
                          loadList();

                          // Reload list missing terms
                          reloadMissingTerms();
                      } else {
                          alert('Error: Pls try again!');
                      }
                      $scope.isDisabledBtnDeleteAll = false;
                  });
              }
          } else {
              alert('Vui lòng chọn chức năng để xóa');
          }
      }

      // Modal create new item
      $scope.openDialogCreateItem = function (roleKey) {

          var modalInstance = $modal.open({
              templateUrl: 'modalCreateTermContent.html',
              controller: 'ModalCreateTermCtrl',
              resolve: {
                  roleKey: function () {
                      return roleKey;
                  }
              }
          });

          modalInstance.result.then(function (success) {
              if (success) {
                  // Reload list
                  loadList();

                  // Reload list missing terms
                  reloadMissingTerms();
              }
          }, function () {
              //$log.info('Modal dismissed at: ' + new Date());
          });
      };

      // Modal edit selected item
      $scope.openDialogEditItem = function (id) {

          var modalInstance = $modal.open({
              templateUrl: 'modalEditTermContent.html',
              controller: 'ModalEditTermCtrl',
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

      // Modal view granted users
      $scope.openDialogViewGrantedUsers = function (id) {

          var modalInstance = $modal.open({
              templateUrl: 'modalViewGrantedUsersContent.html',
              controller: 'ModalViewGrantedUsersCtrl',
              resolve: {
                  itemId: function () {
                      return id;
                  }
              }
          });

          modalInstance.result.then(function (success) {
              if (success) {
                  // Reload list
                  //loadList();

                  // Reload list missing terms
                  //reloadMissingTerms();
              }
          }, function () {
              //$log.info('Modal dismissed at: ' + new Date());
          });
      };
  }])
.controller('ModalCreateTermCtrl', ['$scope', 'termService', '$modalInstance', 'roleKey', function ($scope, termService, $modalInstance, roleKey) {
    $scope.item = {};
    $scope.listRoleOptions = {};

    // Load list role options
    termService.listRoleOptions(function (items) {
        $scope.listRoleOptions = items;
        if (roleKey) {
            $scope.item.RoleKey = roleKey.toString();
            window.setTimeout(function () {
                document.getElementById("name").focus();
            }, 100);
        } else {
            window.setTimeout(function () {
                document.getElementById("roleKey").focus();
            }, 100);
        }

    });

    $scope.createItem = function () {
        if ($scope.addForm.$valid) {
            $scope.isDisabledBtnCreate = true;
            termService.createTerm($scope.item, function (success) {
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
  .controller('ModalEditTermCtrl', ['$scope', 'termService', '$modalInstance', 'itemId', function ($scope, termService, $modalInstance, itemId) {

      $scope.item = {};
      $scope.listRoleOptions = {};

      // Load list role options
      termService.listRoleOptions(function (items) {
          $scope.listRoleOptions = items;
      });

      // Load current term
      termService.getTerm(itemId, function (item) {
          $scope.item = item;
      });

      $scope.updateItem = function () {
          if ($scope.editForm.$valid) {
              $scope.isDisabledBtnUpdate = true;
              termService.saveTerm($scope.item, function (success) {
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
  .controller('ModalViewGrantedUsersCtrl', ['$scope', 'termService', '$modalInstance', 'itemId', function ($scope, termService, $modalInstance, itemId) {

      $scope.item = {};
      $scope.grantedUsers = [];

      // Load current term
      termService.getTerm(itemId, function (item) {
          $scope.item = item;
      });

      // Load list granted users
      termService.getGrantedUsersByTerm(itemId, function (items) {
          $scope.grantedUsers = items;// alert($scope.grantedUsers.length);
      });

      $scope.cancel = function () {
          $modalInstance.dismiss('cancel');
      };
  }]);
