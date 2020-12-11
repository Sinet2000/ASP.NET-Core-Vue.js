import home from 'components/home-page';
import forbidden from 'components/common/error-pages/401-forbidden';
import notFound from 'components/common/error-pages/404-not-found';
import login from 'components/auth/login';
import register from 'components/auth/register';
import forgotPassword from 'components/auth/forgot-password';
import resetPassword from 'components/auth/reset-password';
import confirmEmail from 'components/auth/confirm-email';
import adminUserList from 'components/admin/users/user-list';
import adminCompanyList from 'components/admin/companies/company-list';
import profile from 'components/profile/profile';
import editProfile from 'components/profile/edit-profile';
import manageLogins from 'components/profile/manage-logins';
import changePassword from 'components/profile/change-password';
import setPassword from 'components/profile/set-password';

export const routes = [
    { path: '/', name: 'default', component: home, display: 'Home', meta: { showInMenu: true } },
    { path: '/401-forbidden', name: 'forbidden', component: forbidden, display: '401 Forbidden' },
    { path: '/404-not-found', name: 'not-found', component: notFound, display: '404 Page Not Found' },
    { path: '/login', name: 'login', component: login, display: 'Login' },
    { path: '/register', component: register, display: 'Register' },
    { path: '/auth/forgot-password', component: forgotPassword },
    { path: '/auth/reset-password', component: resetPassword, props: (route) => ({ code: route.query.code }) },
    { path: '/auth/confirm-email', component: confirmEmail, props: (route) => ({ userId: route.query.userId, token: route.query.token }) },
    { path: '/admin/user-list', component: adminUserList, display: 'Users', meta: { showInMenu: true, auth: { roles: 'Admin' } } },
    { path: '/admin/company-list', component: adminCompanyList, display: 'Companies', meta: { showInMenu: true, auth: { roles: 'Admin' } } },

    {
        path: '/profile',
        component: profile,
        display: 'Edit profile',
        meta: { auth: true },
        children: [
            {
                path: '',
                display: 'My Profile',
                component: editProfile,
                meta: { auth: true }
            },
            {
                path: 'manage-logins',
                display: 'External Logins',
                component: manageLogins,
                meta: { auth: true }
            },
            {
                path: 'change-password',
                display: 'Change Password',
                component: changePassword,
                meta: { auth: true }
            },
            {
                path: 'set-password',
                display: 'Set Password',
                component: setPassword,
                meta: { auth: true }
            }
        ]
    },

    { path: '*', redirect: { name: 'not-found' } }
];

export default routes;