var colabConcept = colabConcept || {};
colabConcept.models = colabConcept.models || {};

(function (namespace) {

    var Product = function Product(name, description, id, version, isEditing, editedBy) {
        this.name = name;
        this.description = description;
        this.id = id;
        this.version = version;
        this.isEditing = isEditing,
        this.editedBy = editedBy;

        this.isDeleted = false;
        this.deletedBy = null;
    };

    Product.fromDto = function (dto) {
        return new Product(
            dto.name,
            dto.description,
            dto.id,
            dto.version,
            dto.lockedBy == true,
            dto.editedBy);
    };

    Product.prototype.editedByMe = function editedByMe(id) {
        return this.isEditing && this.editedBy == id;
    }

    Product.prototype.getState = function getState(editingUserId) {
        if (this.isDeleted) {
            return 'deleted';
        }
        else if (this.isEditing) {
            console.log(editingUserId);

            if (this.editedByMe(editingUserId)) {
                return 'editedByMe';
            } else {
                return 'editedByOther';
            }

        }
        else {
            return 'normal';
        }
    };

    namespace.Product = Product;

})(colabConcept.models);

