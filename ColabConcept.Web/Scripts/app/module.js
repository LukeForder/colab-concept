/// <reference path="../moment.js" />
/// <reference path="../angular.js" />
var app = angular.module("app",
    ['ngAnimate'],
    function () { }).
    value('$', $).
    factory(
        'productsService',
        [
            '$http',
            '$q',
            function($http, $q) {
                var create = function (product) {
                    var task = $q.defer();

                    $http.post(
                        'products',
                        product,
                        {
                            headers: {
                                'Accept': 'application/json',
                                'Content-Type': 'application/json'
                            }
                        }).then(
                        function () {
                            task.resolve();
                        },
                        function (error) {
                            console.log(error);
                            task.reject();
                        });

                    return task.promise;
                };

                return {
                    create: create
                };
            }
        ]).
    factory(
        'notificationsService',
        [
            '$',
            '$rootScope',
            '$q',
            function ($, $rootScope, $q) {

                var Product = function(dto) {
                    this.name = dto.Name;
                    this.description = dto.Description;
                    this.id = dto.Id;
                    this.version = dto.Version;
                    this.isEditing = dto.LockedBy,
                    this.editedBy = dto.LockedBy
                };

                Product.prototype.editedByMe = function(id) {
                    return this.isEditing && this.editedBy == id;
                }

                var proxy;

                var initialize = function () {

                    console.log('initialize');
                
                    var connection = $.hubConnection();
                    proxy = connection.createHubProxy('products');
                    connection.start();

                    proxy.on(
                        'connected',
                        function (message) {
                            message.products = message.products.map(function (dto) { return new Product(dto); });

                            $rootScope.$broadcast('system::connected', message);
                        });

                    proxy.on(
                        'reconnected',
                        function (message) {
                            message.products = message.products.map(function (dto) { return new Product(dto); });

                            $rootScope.$broadcast('system::reconnected', message);
                        });

                    proxy.on(
                        'beginEdit',
                        function (message) {
                            $rootScope.$broadcast('product::beginEdit', message);
                        });

                    proxy.on(
                        'joined',
                        function (message) {
                            $rootScope.$broadcast('system::userJoined', message);
                        });

                    proxy.on(
                        'left',
                        function (message) {
                            $rootScope.$broadcast('system::userLeft', message);
                        });

                    proxy.on(
                    'addProduct',
                    function (message) {
                        console.log('addProduct invoked.');
                        $rootScope.$broadcast(
                            'products::new',
                             new Product(message));
                    });

                    proxy.on(
                        'removeProduct',
                        function (message) {
                            $rootScope.$broadcast(
                            'products::remove',
                            {
                                id: message.Id,
                                by: message.By
                            });
                        });

                    proxy.on(
                        'beginEdit',
                        function (message) {
                            $rootScope.$broadcast('products::beginEdit', message);
                        });

                    proxy.on(
                        'commitEdit',
                        function (message) {
                            $rootScope.$broadcast(
                                'products::saveEdit',
                                {
                                    id: message.product.Id,
                                    name: message.product.Name,
                                    description: message.product.Description,
                                    lockedBy: message.product.LockedBy
                                });
                        });

                    proxy.on(
                        'productUnlocked',
                        function (message) {
                            $rootScope.$broadcast(
                               'products::unlocked',
                               {
                                   id: message,
                               });
                        });
                };

                var saveProduct = function (product) {
                    var task = $q.defer();
                    proxy.invoke('saveProduct',
                        {
                            Name: product.name,
                            Description: product.description
                        }).done(
                            function () {
                                task.resolve();
                            }).fail(
                            function (error) {
                                console.log(error);
                                task.reject();
                            });

                    return task.promise;
                };

                var editProduct = function (product) {
                    var task = $q.defer();
                    proxy.invoke('beginEdit',product.id).done(
                            function () {
                                task.resolve();
                            }).fail(
                            function (error) {
                                console.log(error);
                                task.reject();
                            });

                    return task.promise;
                };

                var saveEdit = function (product) {
                    var task = $q.defer();
                    
                    var dto = {
                        id: product.id,
                        name: product.name,
                        description: product.description,
                        lockedBy: product.lockedBy
                    };
                    

                    proxy.invoke('commitEdit', dto).done(
                            function () {
                                task.resolve();
                            }).fail(
                            function (error) {
                                console.log(error);
                                task.reject();
                            });

                    return task.promise;
                };

                var removeProduct = function(product) {
                    var task = $q.defer();
                    proxy.invoke(
                        'removeProduct', product.id).done(
                            function () {
                                task.resolve();
                            }).fail(
                            function (error) {
                                console.log(error);
                                task.reject();
                            });

                    return task.promise;
                };

                
                var cancelEdit = function (product) {
                    var task = $q.defer();
                    proxy.invoke(
                        'cancelEdit', product.id).done(
                            function () {
                                task.resolve();
                            }).fail(
                            function (error) {
                                console.log(error);
                                task.reject();
                            });

                    return task.promise;
                }

                return {
                    initialize: initialize,
                    saveProduct: saveProduct,
                    removeProduct: removeProduct,
                    editProduct: editProduct,
                    saveEdit: saveEdit,
                    cancelEdit: cancelEdit
                };
            }
        ]
    ).
    run([
        'notificationsService',
        function (notificationsService) {
            notificationsService.initialize();
        }
    ]).
    controller(
        'demo',
         [
             '$scope',
             'notificationsService',
             '$interval',
             function ($scope, notificationsService, $interval) {

                 var addingProduct = false;
                 $scope.messages = [];
                 $scope.new = null;
                 $scope.canCreateProduct = function () {
                     return $scope.new == null;
                 }
                 $scope.createProduct = function () {
                     $scope.new = {
                         name: null,
                         description: null
                     };
                 };
                 $scope.canAddProduct = function (product) {
                     return !addingProduct && product && product.name && product.description;
                 };
                 $scope.cancelAddProduct = function () {
                     $scope.new = null;
                 }
                 $scope.userCount = 0;
                 $scope.products = [];
                 $scope.addProduct = function (newProduct) {
                     notificationsService.
                         saveProduct(newProduct).then(function () {
                             addingProduct = false;
                         });                     
                 };
                 $scope.save = function (product) {
                     notificationsService.saveEdit(product);
                 };
                 $scope.delete = function (product) {
                     notificationsService.removeProduct(product);
                 };
                 $scope.cancel = function (product) {
                     if (product.memento) {
                         product.name = product.memento.name;
                         product.description = product.memento.description;
                         product.lockedBy = product.memento.lockedBy;
                     }

                     notificationsService.cancelEdit(product);
                 };
                 $scope.$on('system::connected', function ($event, message) {
                     $scope.$apply(function () {
                         $scope.userCount = message.count;
                         $scope.id = message.id;

                         var m = { kind: 'success', timestamp: Date.now(), content: 'You joined, your id is ' + $scope.id };
                         m.ago = moment(m.timestamp).fromNow();
                         $scope.messages.push(m);
                         
                         $scope.products = message.products;
                     });
                 });
                 $scope.$on('system::reconnected', function ($event, message) {
                     $scope.$apply(function () {
                         $scope.userCount = message.count;
                         $scope.id = message.id;

                         var m = { kind: 'success', timestamp: Date.now(), content: 'The server reset your connection, your id is now ' + $scope.id };
                         m.ago = moment(m.timestamp).fromNow();
                         $scope.messages.push(m);

                         $scope.products = message.products;
                     });
                 });
                 $scope.$on('system::userJoined', function ($event, message) {
                     $scope.$apply(function () {
                         $scope.userCount = message.count;
                         var m = { kind: 'info', timestamp: Date.now(), content: message.id + ' joined' };
                         m.ago = moment(m.timestamp).fromNow();
                         $scope.messages.push(m);
                     });
                 });
                 $scope.$on(
                     'products::saveEdit',
                     function ($event, message) {
                         var product = _.findWhere($scope.products, { id: message.id });
                         product.description = message.description;
                         product.name = message.name;
                         product.lockedBy = message.lockedBy;
                         product.isEditing = false;
                         product.memento = undefined;
                         $scope.$apply();
                     });
                 $scope.$on(
                    'products::unlocked',
                    function ($event, message) {
                        $scope.$apply(function () {
                            var product = _.findWhere($scope.products, { id: message.id });
                            product.lockedBy = null;
                            product.isEditing = false;
                        });
                    });

                 $scope.$on(
                     'products::beginEdit',
                     function ($event, message) {
                         var product = _.findWhere($scope.products, { id: message.product });
                         if (product) {
                             $scope.$apply(
                                 function () {
                                     
                                     product.isEditing = true;
                                     product.editedBy = message.editedBy;
                                     product.memento = product;
                                 });
                         }
                     });

                 $scope.$on('system::userLeft', function ($event, message) {
                     $scope.$apply(function () {
                         $scope.userCount = message.count;
                         var m = { kind: 'warning', timestamp: Date.now(), content: message.id + ' left' };
                         m.ago = moment(m.timestamp).fromNow();
                         $scope.messages.push(m);
                     });
                 });
                 $scope.$on('products::new', function ($event, message) {
                     console.log({ message: message });
                   
                     var product = message;

                     $scope.$apply(function () {

                         $scope.new = {
                             name: null,
                             description: null
                         };

                         $scope.products.push(product);
                     });

                 });
                 $scope.$on(
                     'products::remove',
                     function ($event, message) {
                         console.log(message);
                         var product = _.findWhere($scope.products, { id: message.id });
                         $scope.$apply(function () {
                             product.deleted = true;
                             product.deletedBy = $scope.id === message.by ? 'you' : message.by;
                         });
                     });
                 $scope.edit = function (product) {
                     notificationsService.editProduct(product);
                 }
                 $scope.removeFromList = function (product) {
                     var index = _.indexOf($scope.products, product);
                     if (index > -1) {
                         $scope.products.splice(index, 1);
                     }
                 }
                 var intervalSubscription = $interval(function () {
                     $scope.messages.forEach(function (message) {
                         message.ago = moment(message.timestamp).fromNow();
                     });
                 }, 44000)
             }
         ]);
