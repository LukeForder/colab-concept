var colabConcept = colabConcept || {};
colabConcept.directives = colabConcept.controllers || {};

(function messageDirectiveModule(namespace) {

    var MessageDirective = function MessageDirective(baseUrl) {
        console.log('new message directive created');
        
        this.restrict = "A";
        this.templateUrl =  baseUrl + '/scripts/app/partials/message.html';
        this.scope =  {
            content: '=',
            kind: '=',
            ago: '='
        };
    };

    MessageDirective.prototype.link = function (scope, element, attrs) {
        console.log(scope);

    };

    var MessageDirectiveFactory = function MessageDirectiveFactory(baseUrl) {
        return new MessageDirective(baseUrl);
    };


    namespace.MessageDirectiveFactory = MessageDirectiveFactory;

})(colabConcept.directives);