var colabConcept = colabConcept || {};
colabConcept.services = colabConcept.services || {};

(function notificationsServiceModule(namespace) {

    var NotificationsService = function notificationsService($, $rootScope, $q) {

        Object.defineProperties(
            this,
            {
                signalr: {
                    get: function get() {
                        return $;
                    }
                },
                rootScope: {
                    get: function get() {
                        return $rootScope;
                    }
                },
                qService: {
                    get: function get() {
                        return $q;
                    }
                },
                proxy: {
                    value: null,
                    writable: true
                }
            });

    };

    NotificationsService.prototype.initialize = function initialize() {

        console.log('initialize');
                
        function mapToModels (dtos) {
            return dtos.map(
                function mapFromDtoToModel(dto) {
                    return colabConcept.models.Product.fromDto(dto); 
                });
        };

        var connection = this.signalr.hubConnection();
        this.proxy = connection.createHubProxy('products');
        connection.start();

        var that = this;

        this.proxy.on(
            'connected',
            function onProxyConnected(message) {
                message.products = mapToModels(message.products)
                that.rootScope.$broadcast('system::connected', message);
            });

        this.proxy.on(
            'reconnected',
            function onProxyReconnected(message) {
                message.products = mapToModels(message.products);
                that.rootScope.$broadcast('system::reconnected', message);
            });

        this.proxy.on(
            'beginEdit',
            function onBeginEdit(message) {
                that.rootScope.$broadcast('products::beginEdit', message);
            });

        this.proxy.on(
            'joined',
            function onJoined(message) {
                that.rootScope.$broadcast('system::userJoined', message);
            });

        this.proxy.on(
            'left',
            function onLeft(message) {
                that.rootScope.$broadcast('system::userLeft', message);
            });

        this.proxy.on(
            'addProduct',
            function onAddProduct(message) {
                that.rootScope.$broadcast(
                    'products::new',
                    {
                        addedBy: message.addedBy,
                        product: colabConcept.models.Product.fromDto(message.product)
                    });
            });

        this.proxy.on(
            'removeProduct',
            function onRemoveProduct(message) {
                that.rootScope.$broadcast(
                    'products::remove',
                    {
                        id: message.Id,
                        by: message.By
                    });
            });

        this.proxy.on(
            'commitEdit',
            function onCommitEdit(message) {
                that.rootScope.$broadcast(
                    'products::saveEdit',
                    message.product);
            });

        this.proxy.on(
            'productUnlocked',
            function onProductUnlocked(message) {
                that.rootScope.$broadcast(
                    'products::unlocked',
                    {
                        id: message,
                    });
            });
    };

    NotificationsService.prototype.saveProduct = function saveProduct(product) {

        function onProductSavedSuccess() {
            task.resolve();
        }

        function onProductSavedFailed(response) {
            console.log(response);
            task.reject();
        }

        var task = this.qService.defer();

        this.proxy.invoke(
            'saveProduct',
            {
                Name: product.name,
                Description: product.description
            }).done(onProductSavedSuccess).fail(onProductSavedFailed);

            return task.promise;
    };

    NotificationsService.prototype.editProduct = function editProduct(product) {

        function onBeginEditSucceeded() {
            task.resolve();
        }

        function onBeginEditFailed(response) {
            console.log(response);
            task.reject();
        }

        var task = this.qService.defer();

        this.proxy.invoke('beginEdit',product.id).done(onBeginEditSucceeded).fail(onBeginEditFailed);

        return task.promise;
    };

    NotificationsService.prototype.saveEdit = function saveEdit(product) {
           
        function onSaveEditSuccess() {
            task.resolve();
        }

        function onSaveEditFailed() {
            console.log(error);
            task.reject();
        }

        var task = this.qService.defer();
                    
        var dto = {
            id: product.id,
            name: product.name,
            description: product.description,
            lockedBy: product.lockedBy
        };
        
        this.proxy.invoke('commitEdit', dto).done(onSaveEditSuccess).fail(onSaveEditFailed);

        return task.promise;
    };

    NotificationsService.prototype.removeProduct = function removeProduct(product) {
        
        function onRemoveProductSucceeded() {
            task.resolve();
        }

        function onRemoveProductFailed(response) {
            console.log(response);
            task.reject();
        }

        var task = this.qService.defer();
        this.proxy.invoke('removeProduct', product.id).done(onRemoveProductSucceeded).fail(onRemoveProductFailed);

        return task.promise;
    };

    NotificationsService.prototype.cancelEdit = function cancelEdit(product) {

        function onCancelEditSuccess() {
            task.resolve();
        }

        function onCancelEditFailed(response) {
            console.log(response);
            task.reject();
        }

        var task = this.qService.defer();
        
        this.proxy.invoke('cancelEdit', product.id).done(onCancelEditSuccess).fail(onCancelEditFailed);

        return task.promise;
    };

    namespace.NotificationsService = NotificationsService;

})(colabConcept.services);