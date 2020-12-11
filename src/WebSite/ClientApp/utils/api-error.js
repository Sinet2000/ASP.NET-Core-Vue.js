class ApiError extends Error {
    constructor(exception) {

        let customErrors = ApiError.collectCustomErrors(exception.response);

        super(customErrors.length > 0 ? customErrors[0] : exception.message);

        this.customErrors = customErrors;

        if (Error.captureStackTrace)
            Error.captureStackTrace(this, ApiError);
        else
            this.toString = () => { return this.name + ': ' + this.message }
    }

    static collectCustomErrors(response) {

        if (response === undefined)
            return [];

        if (response.status !== 400)
            return [];

        //check if custom error message was returned
        if (response.data && (response.data.error_description || response.data.error)) {
            let errorMessage = response.data.error_description ? response.data.error_description : response.data.error;

            if (Array.isArray(errorMessage) && errorMessage.length)
                return errorMessage;

            if (typeof errorMessage === "string")
                return [errorMessage];
        }

        //custom error message was not found
        return [];
    }
}

export default ApiError;