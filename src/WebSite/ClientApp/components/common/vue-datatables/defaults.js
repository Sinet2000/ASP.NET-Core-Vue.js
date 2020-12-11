const dtOptions = {
    tableClass: 'table table-striped table-responsive-lg dtr-inline',

    selectable: {
        selectAllCheckboxTemplate: (id) => `<div class='custom-control custom-checkbox'><input type='checkbox' id='${id}' class='custom-control-input' /><label class='custom-control-label' for='${id}'></label></div>`,
        checkboxTemplate: (id, cssClass, data, checked) => `<div class='custom-control custom-checkbox'><input type='checkbox' id='${id}' class='custom-control-input ${cssClass}' data-id='${data}' ${checked} /><label class='custom-control-label' for='${id}'></label></div>`
    },

    autoWidth: false,
    order: [0, 'asc'],
    serverSide: true,

    dom: "rt<'row'<'col-sm-6'i><'col-sm-6'p>>",

    pageLength: 5,
    lengthMenu: [5, 10, 25, 50, 75, 100]
};

const classes = {
    sWrapper: "dataTables_wrapper",
    sFilterInput: "form-control form-control-sm",
    sLengthSelect: "form-control form-control-sm",
    sProcessing: "dataTables_processing card"
};

export default {
    dtOptions,
    classes
}