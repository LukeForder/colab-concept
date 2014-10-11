/// <reference path="../moment.js" />
/// <reference path="../angular.js" />
/// <reference path="models/Product.js" />
/// <reference path="services/notificationsService.js" />
/// <reference path="controllers/demoController.js" />
/// <reference path="directives/message.js" />
/// <reference path="services/productsService.js" />
var app = angular.module("app", []).
    value('$', $).
    constant('baseUrl', "").
    service(
        'productsService', 
        [
            '$http',
            '$q',
            colabConcept.services.ProductsService
        ]).
    service(
        'notificationsService',
        [
            '$',
            '$rootScope',
            '$q',
            colabConcept.services.NotificationsService
        ]).
    run([
        'notificationsService',
        // ensure that the hub proxy is initialized
        function (notificationsService) {
            notificationsService.initialize();
        }
    ]).
    directive("message",
    [
        'baseUrl',
        colabConcept.directives.MessageDirectiveFactory
    ]).
    controller(
        'demo',
         [
             '$scope',
             'notificationsService',
             '$interval',
              colabConcept.controllers.DemoController
         ]);
