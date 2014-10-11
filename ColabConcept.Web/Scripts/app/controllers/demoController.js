var colabConcept = colabConcept || {};
colabConcept.controllers = colabConcept.controllers || {};

(function (namespace) {
    
    var NewProduct = function(name, description) {
        this.name = name;
        this.description = description;
    }

    var Message = function Message(kind, timestamp, content) {
        this.kind = kind;
        this.timestamp = timestamp;
        this.content = content;
    };

    // TODO: this is a bad approach... undeclared dependency
    Message.prototype.ago = function() {
        return moment(this.timestamp).fromNow();
    }

    var DemoController = function ($scope, notificationsService, $interval) {

        Object.defineProperties(
            this,
            {
                scope: {
                    value: $scope
                },
                notificationsService: {
                    value: notificationsService
                },
                intervalService: {
                    value: $interval
                },
                messages: {
                    value: [],
                    writable: true
                },
                newProduct: {
                    value: null,
                    writable: true
                },
                addingProduct: {
                    value: false,
                    writable: true
                },
                signOnId: {
                    value: null,
                    writable: true
                },
                userCount: {
                    value: 0,
                    writable: true
                },
                products: {
                    value: [],
                    writable: true
                }
            });

        var that = this;

        that.scope.$on(
            'system::connected',
            function systemConnectedHandler($event, message) {
                that.scope.$apply(function onSystemConnected() {

                    var displayMessage = new Message('success', Date.now(), 'You joined, your id is ' + message.id);

                    that.userCount = message.count;
                    that.signOnId = message.id;                                  
                    that.products = message.products;

                    that.messages.push(displayMessage);           
                });
            }
        );

        that.scope.$on(
            'system::reconnected', 
            function systemReconnectedHandler($event, message) {
                that.scope.$apply(
                    function onSystemReconnected() {
                        that.userCount = message.count;
                        that.signOnId = message.id;

                        var displayMessage = new Message('success', Date.now(), 'The server reset your connection, your id is now ' + that.scope.id );

                        that.messages.push(displayMessage);
                        that.products = message.products;
                    });
                });

        that.scope.$on(
            'system::userJoined', 
            function systemUserJoinedHandler($event, message) {
                that.scope.$apply(
                    function onSystemUserJoined() {
                        that.userCount = message.count;

                        var displayMessage = new Message('info', Date.now(), message.id + ' joined' );

                        that.messages.push(displayMessage);
                });
            });

        that.scope.$on(
            'products::saveEdit',
            function productsSaveEditHandler($event, message) {
                that.scope.$apply(
                    function onProductsSaveEdit() {

                        var product = _.findWhere(
                            that.products, 
                            { 
                                id: message.id 
                            });

                        product.description = message.description;
                        product.name = message.name;
                        product.lockedBy = message.lockedBy;
                        product.isEditing = false;
                        product.memento = undefined;
                    });
            });

        that.scope.$on(
           'products::unlocked',
           function productsUnlockedHandler($event, message) {
               that.scope.$apply(
                   function onProductUnlocked() {

                       var product = _.findWhere(
                           that.products, 
                           { 
                               id: message.id 
                           });

                       product.lockedBy = null;
                       product.isEditing = false;
                });
        });

        that.scope.$on(
            'products::beginEdit',
            function productsBeginEdit($event, message) {

                var product = _.findWhere(
                    that.products, 
                    { 
                        id: message.product 
                    });

                if (product) {
                    that.scope.$apply(
                        function () {            
                            product.isEditing = true;
                            product.editedBy = message.editedBy;
                            product.memento = product;
                        });
                }
            });

        that.scope.$on(
            'system::userLeft',
            function systemUserLeftHandler($event, message) {
                that.scope.$apply(
                    function onSystemUserLeft() {
                        that.userCount = message.count;

                        var displayMessage = new Message('warning', Date.now(), message.id + ' left');

                        that.messages.push(displayMessage);
                    });
            });

        that.scope.$on(
            'products::new',
            function productsNewHandler($event, message) {
                   
                var product = message.product;

                that.scope.$apply(
                    function onProductsNew() {
                    if (that.signOnId === message.addedBy) {
                        that.newProduct = new NewProduct(null, null);
                    }
                    that.products.push(product);
                });
            });

        that.scope.$on(
            'products::remove',
            function productsRemoveHandler($event, message) {

                var product = _.findWhere(
                    that.products,
                    {
                        id: message.id
                    });

                that.scope.$apply(
                    function onProductsRemove() {
                        product.isDeleted = true;
                        product.deletedBy = $scope.id === message.by ? 'you' : message.by;
                    });
            });
    };

    DemoController.prototype.canCreateProduct = function () {
        return this.newProduct == null;
    };

    DemoController.prototype.createProduct = function () {
        this.newProduct = new NewProduct();
    };
    
    DemoController.prototype.cancelAddProduct = function () {
        this.newProduct = null;
    };

    DemoController.prototype.edit = function (product) {
        this.notificationsService.editProduct(product);
    };

    DemoController.prototype.removeFromList = function (product) {
        var index = _.indexOf(this.products, product);

        if (index > -1) {
            this.products.splice(index, 1);
        }
    };

    DemoController.prototype.addProduct = function (newProduct) {
        var that = this;

        that.
        notificationsService.
        saveProduct(newProduct).
        then(function onAddProductSuccess() {
            that.addingProduct = false;
        });                     
    };

    DemoController.prototype.save = function (product) {
        this.notificationsService.saveEdit(product);
    };

    DemoController.prototype.delete = function (product) {
        this.notificationsService.removeProduct(product);
    };

    DemoController.prototype.cancel = function (product) {

        if (product.memento) {
            product.name = product.memento.name;
            product.description = product.memento.description;
            product.lockedBy = product.memento.lockedBy;
        }

        this.notificationsService.cancelEdit(product);
    };

    namespace.DemoController = DemoController;

})(colabConcept.controllers);