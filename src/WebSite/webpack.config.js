const path = require("path");
const webpack = require("webpack");

const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const VueLoaderPlugin = require('vue-loader/lib/plugin');
const UglifyJsPlugin = require("uglifyjs-webpack-plugin");
const OptimizeCSSAssetsPlugin = require("optimize-css-assets-webpack-plugin");
//const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;
//const Visualizer = require('webpack-visualizer-plugin');

const outputPath = path.join(__dirname, "wwwroot/build");

module.exports = function (isDevMode) {

    const config = {
        mode: isDevMode ? 'development' : 'production',
        devtool: isDevMode ? 'inline-cheap-module-source-map' : false,

        entry: {
            common: [
                "jquery",
                "bootstrap-loader",
                "font-awesome/scss/font-awesome.scss",
                "open-iconic/font/css/open-iconic-bootstrap.css",
                'event-source-polyfill',
                'vue',
                'vuex',
                'axios',
                'vue-router',
                "boot-app.js"
            ]
        },

        output: {
            publicPath: "/build/",
            path: outputPath,
            filename: "[name].js",
            chunkFilename: isDevMode ? "[name].js" : "[name].[chunkhash].js"
        },

        module: {
            rules: [
                { parser: { amd: false } },

                // JS linter
                { test: /\.js$/, include: /ClientApp/, use: [{ loader: "eslint-loader", options: { configFile: path.join(__dirname, ".eslintrc") } }], enforce: "pre" },

                // vue
                { test: /\.vue$/, use: 'vue-loader' },

                // babel
                { test: /\.js$/, include: /ClientApp/, loader: "babel-loader" },

                {
                    test: /\.(css|scss)/,
                    use: [
                        MiniCssExtractPlugin.loader,
                        { loader: "css-loader", options: { sourceMap: true } },
                        { loader: "postcss-loader", options: { sourceMap: true } },
                        { loader: "resolve-url-loader", options: { sourceMap: true } },
                        { loader: "sass-loader", options: { sourceMap: true } }
                    ]
                },

                // copy images and fonts from source folder into output directory
                {
                    test: /\.(png|jpg|ico|woff2?|svg|ttf|eot|otf|)$/,
                    exclude: /ClientApp\\components\\svg\\*/,
                    use: [
                        {
                            loader: "url-loader",
                            options: {
                                limit: 1000,
                                name: "assets/[path][name].[ext]"
                            }
                        }
                    ]
                },

                //vue-svg-loader
                //{
                //    test: /\.svg$/,
                //    loader: 'vue-svg-loader',
                //    options: { svgo: { plugins: [{ removeDoctype: true }, { removeComments: true }] } }
                //}
            ]
        },

        resolve: {
            symlinks: false, //required for "npm link" to work
            modules: ['ClientApp', 'node_modules'],
            extensions: ['.js', '.vue'],
            alias: {
                'vue$': 'vue/dist/vue.runtime.esm.js',
                'components': path.resolve(__dirname, './ClientApp/components'),
                'views': path.resolve(__dirname, './ClientApp/views'),
                'utils': path.resolve(__dirname, './ClientApp/utils'),
                'api': path.resolve(__dirname, './ClientApp/store/api')
            }
        },

        optimization: {
            splitChunks: {
                chunks: 'async'
            }
        },

        plugins: [
            new VueLoaderPlugin(),
            new webpack.ProvidePlugin({
                $: 'jquery',
                jQuery: 'jquery'
            }),
            new MiniCssExtractPlugin({
                filename: "[name].css",
                chunkFilename: isDevMode ? "[name].css" : "[name].[chunkhash].css"
            }),
            new webpack.IgnorePlugin(/^\.\/locale$/, /moment$/)
        ]
    };

    if (!isDevMode) {
        config.optimization.minimizer = [
            new UglifyJsPlugin({
                cache: true,
                parallel: true,
                sourceMap: false // set to true if you want JS source maps
            }),
            new OptimizeCSSAssetsPlugin({})
        ];

        //config.plugins = config.plugins.concat(
        //    new BundleAnalyzerPlugin(),
        //    new Visualizer()
        //);
    }

    return config;
};