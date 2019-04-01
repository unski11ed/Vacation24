import cookies from 'js-cookie';
import queryString from 'query-string';

export function getUserData() {
    var cookieData = cookies.get('UserData');
    var userData = Object.assign({ }, {
        IsLoggedIn: false,
        UserId: 0,
        Role: ''
    }, queryString.parse(cookieData));

    return userData;
}