<template>
    <div>
        <div class="page-header">
            <h1>Welcome</h1>
        </div>

        <p>Main technologies used:</p>

        <ul class="list-unstyled">
            <li><a href="http://vuejs.org">Vue.js</a></li>
            <li><a href="http://getbootstrap.com">Twitter for Bootstrap</a> + <a href="http://bootstrap-vue.js.org">Bootstrap-Vue</a></li>
            <li><a href="http://webpack.github.io">Webpack</a></li>
            <li><a href="http://babeljs.io">Babel</a></li>
            <li><a href="http://eslint.org">ESLint</a></li>
            <li><a href="http://automapper.org">AutoMapper</a></li>
            <li><a href="http://datatables.net">DataTables</a></li>
        </ul>

        <div class="page-header">
            <h1>Accounts</h1>
        </div>

        <p>Default admin account is admin@example.com / qwerty</p>

        <div class="page-header">
            <h1>Running Webpack</h1>
        </div>

        <p>For development, run "webpack:start-dev-server" Gulp task from Task Runner Explorer. Webpack dev server's port can be changed in appsettings.json (default is 9999).</p>

        <div class="page-header">
            <h1>Executing Migrations</h1>
        </div>

        <p>Entity Framework command-line tools (<i>dotnet ef</i>) has to be executed from BusinessLogic folder. When executing migration commands, you need to specify the <i>--startup-project</i> option, e.g.:</p>
        <p><i>dotnet ef --startup-project ../WebSite migrations list</i></p>
        <p>More information is available in <a href="https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet">EF command-line tools documentation</a>.</p>
    </div>
</template>

<script>
export default {
    data() {
        return {
        }
    },
}
</script>

<style>
</style>