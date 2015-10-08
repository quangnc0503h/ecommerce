'use strict';

/**
 * @ngdoc function
 * @name authclientApp.controller:UserCtrl
 * @description
 * # UserCtrl
 * Controller of the authclientApp
 */
angular.module('quangauthwebApp')
  .controller('UserCtrl', ['$scope', '$modal', '$location', '$routeParams', 'userService', 'groupService', 'localDataService', function ($scope, $modal, $location, $routeParams, userService, groupService, localDataService) {
      var pagekey = 'UserCtrlPageInfo' + ($routeParams.groupId ? $routeParams.groupId : '');
      var pageInfo = { qsearch: '', groupId: ($routeParams.groupId ? parseInt($routeParams.groupId) : ''), currentPage: 1, itemsPerPage: 10 };
      if (localDataService.get(pagekey) !== null) {
          pageInfo = localDataService.get(pagekey);
      }
      
      $scope.hideBtnBack = $routeParams.groupId ? true : false;
      $scope.items = {};
      $scope.qsearch = pageInfo.qsearch;
      $scope.GroupId = pageInfo.groupId;
      $scope.currentPage = pageInfo.currentPage;
      $scope.itemsPerPage = pageInfo.itemsPerPage;
      $scope.totalItems = 0;

      // Load list function
      var loaded = false;
      var loadList = function () {
          var filter = {
              Keyword: $scope.qsearch,
              GroupId: $scope.GroupId,
              PageSize: $scope.itemsPerPage,
              PageNumber: $scope.currentPage - 1,
              OrderBy: '',
          }
          userService.listUser(filter, function (res) {
              //console.log(res);
              $scope.items = res.items;
              $scope.totalItems = res.totalCount;
              loaded = true;
              $scope.currentPage = pageInfo.currentPage;
          });
          loaded = false;
      };

      // Load list groups
      groupService.listAllOptions(function (items) {
          $scope.listGroups = items;
      });

      // On ready should load list
      loadList();

      // Action when click button search
      $scope.searchItems = function () {
          $scope.currentPage = pageInfo.currentPage = 1;
          pageInfo.qsearch = $scope.qsearch;
          pageInfo.groupId = $scope.GroupId;
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
          if (confirm("Bạn có chắc muốn xóa thành viên này?")) {
              $scope["isDisabledBtnDelete" + itemId] = true;
              userService.removeUser(itemId, function (success) {
                  if (success) {
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
              if (confirm("Bạn có chắc muốn xóa các thành viên đã chọn?")) {
                  $scope.isDisabledBtnDeleteAll = true;
                  userService.removeListUser(checkedItems, function (success) {
                      if (success) {
                          loadList();
                      } else {
                          alert('Error: Pls try again!');
                      }
                      $scope.isDisabledBtnDeleteAll = false;
                  });
              }
          } else {
              alert('Vui lòng chọn thành viên để xóa');
          }
      }

      // Modal setting app for selected item
      $scope.openDialogSettingApp = function (item) {

          var modalInstance = $modal.open({
              templateUrl: 'modalSettingAppContent.html',
              controller: 'ModalSettingAppCtrl',
              resolve: {
                  user: function () {
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

      // Modal grant user permission
      $scope.openDialogGrantUser = function (item) {

          var modalInstance = $modal.open({
              templateUrl: 'modalGrantUserContent.html',
              controller: 'ModalGrantUserCtrl',
              resolve: {
                  user: function () {
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

      // Link to edit page
      $scope.goToEditItem = function (id) {
          if (id) {
              $location.path('/user/edit/' + id);
          } else {
              $location.path('/user/add');
          }
      };

      // Link to grant user page
      $scope.goToGrantItem = function (id) {
          $location.path('/user/grant/' + id);
      }

      // Back to group list
      $scope.goToBack = function () {
          $location.path('/group');
      }
  }])
  .controller('ModalSettingAppCtrl', ['$scope', 'userService', '$modalInstance', 'user', function ($scope, userService, $modalInstance, user) {

      $scope.user = user;
      $scope.userApp = {};

      userService.getUserClientApp(user.Id, function (userApp) {
          if (userApp !== null) {
              $scope.userApp = userApp;
          } else {
              $scope.userApp = { IsActive: 0, UserId: user.Id };
          }
      });

      $scope.updateUserApp = function () {
          if ($scope.editForm.$valid) {
              $scope.isDisabledBtnSettingApp = true;
              userService.updateUserApp($scope.userApp, function (success) {
                  if (success) {
                      $modalInstance.close(true);
                  } else {
                      alert('Error: Pls try again!');
                  }
                  $scope.isDisabledBtnSettingApp = false;
              });
          }
      };

      $scope.refreshApiKey = function () {
          userService.generateApiKey(function (api) {
              $scope.userApp.ApiKey = api.ApiKey;
          });
      };

      $scope.refreshApiSecret = function () {
          userService.generateApiKey(function (api) {
              $scope.userApp.ApiSecret = api.ApiSecret;
          });
      };

      $scope.cancel = function () {
          $modalInstance.dismiss('cancel');
      };
  }])
  .controller('ModalGrantUserCtrl', ['$scope', 'userService', 'permissionService', '$modalInstance', 'user', function ($scope, userService, permissionService, $modalInstance, user) {

      $scope.user = user;
      $scope.userPermissions = [];

      userService.getUserPermissions(user.Id, function (userPermissions) {
          console.log(userPermissions);
          $scope.userPermissions = userPermissions;
      });

      $scope.updateUserPermissions = function () {
          if ($scope.editForm.$valid) {
              $scope.isDisabledBtnGrant = true;
              var permissions = [];
              angular.forEach($scope.userPermissions, function (item) {
                  if (item.IsGranted) {
                      permissions.push(item.Permission.Id);
                  }
              });
              userService.updateUserPermissions(user.Id, permissions, function (success) {
                  if (success) {
                      $modalInstance.close(true);
                  } else {
                      alert('Error: Pls try again!');
                  }
                  $scope.isDisabledBtnGrant = false;
              });
          }
      };
      
      // Action when click check all
      $scope.checkAll = function () {
          if ($scope.selectedAll) {
              $scope.selectedAll = true;
          } else {
              $scope.selectedAll = false;
          }
          angular.forEach($scope.userPermissions, function (item) {
              item.IsGranted = $scope.selectedAll;
          });
      };

      $scope.cancel = function () {
          $modalInstance.dismiss('cancel');
      };
  }])
  .controller('UserEditCtrl', ['$scope', '$location', '$routeParams', 'userService', 'groupService', function ($scope, $location, $routeParams, userService, groupService) {
    $scope.item = { Status: 1, UserGroups: [] };
    $scope.listGroups = [];

    groupService.listAllOptions(function (items) {
        $scope.listGroups = items;
    });

    if ($routeParams.id) {
        userService.getUser($routeParams.id, function (item) {
            $scope.item = item;
        });
    }

    $scope.updateUser = function () {
        if ($scope.editForm.$valid) {
            $scope.isDisabledBtnUpdate = true;
            var updatefn = $routeParams.id ? userService.saveUser : userService.createUser;
            updatefn($scope.item, function (success) {
                if (success) {
                    $location.path('/user');
                } else {
                    alert('Error: Pls try again!');
                }
                $scope.isDisabledBtnUpdate = false;
            });
        }
    };

    $scope.removeItem = function (i) {
        var newItems = [];
        angular.forEach($scope.item.UserGroups, function (item, index) {
            if (index != i) {
                newItems.push(item);
            }
        });
        $scope.item.UserGroups = newItems;
    }

    $scope.addItem = function (i) {
        var newItems = [];
        if (i >= 0) {
            angular.forEach($scope.item.UserGroups, function (item, index) {
                newItems.push(item);
                if (index == i) {
                    newItems.push({ Id: '' });
                }
            });
        } else {
            newItems.push({ Id: '' });
        }
        $scope.item.UserGroups = newItems;
    };

    $scope.backToList = function () {
        $location.path('/user');
    }
  }])
  .controller('UserGrantCtrl', ['$scope', '$location', '$routeParams', 'userService', 'groupService', 'termService', function ($scope, $location, $routeParams, userService, groupService, termService) {
    $scope.items = [];
    $scope.user = {};

    userService.getUser($routeParams.id, function (user) {
        $scope.user = user;
    });
    
    termService.getGrantUserTerms($routeParams.id, function (items) {
        $scope.items = items;
    });

    // Action when click update grant
    $scope.updateGrant = function () {
        if (true || confirm("Bạn đã chắc chắn thực hiện phân quyền?")) {
            termService.updateUserGrant($routeParams.id, $scope.items, function (success) {
                if (success) {
                    alert("Đã thực hiện phân quyền thành công");
                } else {
                    alert('Error: Pls try again!');
                }
            });
        }
    };

    // Action when click check custom all
    $scope.checkIsCustomAll = function () {
        if ($scope.isCustomAll) {
            $scope.isCustomAll = true;
        } else {
            $scope.isCustomAll = false;
        }
        angular.forEach($scope.items, function (item) {
            item.IsCustom = $scope.isCustomAll;
        });
    };

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
        $location.path('/user');
    }

    $scope.backToList = function () {
        $location.path('/user');
    }
}]);
