var config = {
  auth: {
    clientId: '72b94666-c0f8-451b-a060-e48ab230c13c',
    authority: 'https://login.microsoftonline.com/8e81e7c5-f030-4a29-85be-a64c83c892b6',
    redirectUri: 'http://localhost:4000/recipes/'
  }
  // cache: {
  // cacheLocation: 'localStorage',
  // storeAuthStateInCookie: true
  // }
};
var msal = new Msal.UserAgentApplication(config);

if (msal.getAccount() && !msal.isCallback(window.location.hash)) {
  // avoid duplicate code execution on page load in case of iframe and popup window.
  alert('welcome logic guard');

  sessionStorage.setItem('recipes.user', msal.getAccount().name);
  acquireTokenRedirectAndCallMSGraph(graphConfig.graphMeEndpoint, loginRequest);
}

function authRedirectCallBack(error, response) {
  if (error) {
    console.log(error);
  } else {
    if (response.tokenType === 'id_token' && myMSALObj.getAccount() && !myMSALObj.isCallback(window.location.hash)) {
      alert('welcome');
      acquireTokenRedirectAndCallMSGraph(graphConfig.graphMeEndpoint, loginRequest);
    } else if (response.tokenType === 'access_token') {
      callMSGraph(graphConfig.graphMeEndpoint, response.accessToken, graphAPICallback);
    } else {
      console.log('token type is:' + response.tokenType);
    }
  }
}

function acquireTokenRedirectAndCallMSGraph(endpoint, request) {
  //Call acquireTokenSilent (iframe) to obtain a token for Microsoft Graph
  msal
    .acquireTokenSilent(request)
    .then(function(tokenResponse) {
      alert(tokenResponse);
      // callMSGraph(endpoint, tokenResponse.accessToken, graphAPICallback);
    })
    .catch(function(error) {
      console.log('error is: ' + error);
      console.log('stack:' + error.stack);
      //Call acquireTokenRedirect in case of acquireToken Failure
      if (requiresInteraction(error.errorCode)) {
        myMSALObj.acquireTokenRedirect(request);
      }
    });
}

msal.handleRedirectCallback(authRedirectCallBack);
var loginRequest = {
  scopes: ['user.read'] // optional Array<string>
};

function getToken(tokenRequest) {
  return msal.acquireTokenSilent(tokenRequest).catch(function(error) {
    alert('aquire token popup');
    // fallback to interaction when silent call fails
    return msal
      .acquireTokenPopup(tokenRequest)
      .then(function(tokenResponse) {})
      .catch(function(error) {
        alert('Failed token acquisition', error);
      });
  });
}

window.onload = function() {
  let loginButton = document.getElementById('loginButton');
  loginButton.onclick = function() {
    msal.loginRedirect(loginRequest);
    // .then(function (loginResponse) {
    // 	console.log(loginResponse);
    // 	getToken(tokenRequest).then(alert('logged in'));
    // }).catch(function (error) {
    // 	alert(error);
    // });

    return false;
  };
  let user = sessionStorage.getItem('recipes.user');
  if (user) {
    document.getElementById('username').textContent = ` (${user})`;

    document.getElementById('editButton').style.display = 'inherit';
    loginButton.style.display = 'none';
  }
};
