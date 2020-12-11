/// <binding />
const gulp = require('gulp');
const gutil = require('gulp-util');
const del = require('del');
const path = require("path");
const webpack = require('webpack');
const WebpackDevServer = require("webpack-dev-server");
const WebpackRequireFrom = require('webpack-require-from');

const configFunc = require('./webpack.config.js');
const appConfig = require('./appsettings.json');

gulp.task('webpack:build-prod', function (callback) {

    const config = configFunc(false);

    del(config.output.path);

    // run webpack
    webpack(config, function (err, stats) {
        if (err)
            throw new gutil.PluginError('webpack', err);

        gutil.log('[webpack]', stats.toString({
            colors: true
        }));

        callback();
    });
});

gulp.task("webpack:start-dev-server", function (callback) {

    const config = configFunc(true);

    var asyncChunksUrl = "http://localhost:" + appConfig.DevelopmentSettings.WebpackDevServerPort + config.output.publicPath;

    config.plugins = config.plugins.concat(
        new WebpackRequireFrom({ path: asyncChunksUrl })
    );

    const options = {
        publicPath: config.output.publicPath,
        contentBase: path.join(__dirname, 'wwwroot'),
        stats: {
            colors: true
        },
        inline: false,
        headers: { "Access-Control-Allow-Origin": "*" }
    };

    WebpackDevServer.addDevServerEntrypoints(config, options);

    var compiler = webpack(config);

    // start a webpack-dev-server
    new WebpackDevServer(compiler, options)
        .listen(appConfig.DevelopmentSettings.WebpackDevServerPort, "localhost", function (err) {
            if (err)
                throw new gutil.PluginError("webpack-dev-server", err);

            gutil.log("[webpack-dev-server]", "http://localhost:" + appConfig.DevelopmentSettings.WebpackDevServerPort + "/webpack-dev-server/");
        });
});