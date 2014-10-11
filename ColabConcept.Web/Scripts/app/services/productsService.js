var colabConcept = colabConcept || {};
colabConcept.services = colabConcept.services || {};
(function productsServiceModule(appNamespace) {
    var productsService = function productsService($http, $q) {

        Object.defineProperties(
            this, {
                httpService: {
                    get: function () {
                        return $http;
                    }
                },
                qService: {
                    get: function () {
                        return $q
                    }
                }
            });
    };

    productsService.prototype.create = function (product) {

        var task = this.qService.defer();

        var onCreateSuccess = function onCreateSuccess() {
            task.resolve();
        };

        var onCreateFailed = function onCreateFailed(response) {
            console.log(response);
            task.reject();
        }

        this.httpService.post(
            'products',
            product,
            {
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                }
            }).then(onCreateSuccess, onCreateFailed);

            return task.promise;
        };

    appNamespace.ProductsService = productsService;

})(colabConcept.services);