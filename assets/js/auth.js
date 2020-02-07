var msalConfig = {
  auth: {
    clientId: '72b94666-c0f8-451b-a060-e48ab230c13c',
    authority: 'https://login.microsoftonline.com/8e81e7c5-f030-4a29-85be-a64c83c892b6',
    redirectUri: authCallback
  },
  cache: {
    cacheLocation: 'sessionStorage',
    storeAuthStateInCookie: true
  }
};
const loginRequest = {
  scopes: ['openid', 'profile', 'User.Read']
};
// Select DOM elements to work with
const userElement = document.getElementById('username');
const loginButton = document.getElementById('loginButton');
// const jsonPre = document.getElementById('json');

const graphConfig = {
  graphMeEndpoint: 'https://graph.microsoft.com/v1.0/me'
};

const requestObj = {
  scopes: ['user.read']
};

// Create the main myMSALObj instance
const myMSALObj = new Msal.UserAgentApplication(msalConfig);

//???
if (location.hash.includes('id_token') || location.hash.includes('access_token')) {
  open(location, '_self').close();
  window.close();
}

loginButton.addEventListener('click', signIn, false);

// Register Callbacks for redirect flow
myMSALObj.handleRedirectCallback(authRedirectCallBack);

async function signIn() {
  try {
    await myMSALObj.loginPopup(requestObj);

    // Login Success

    window.close();
    showWelcomeMessage();
    acquireTokenPopupAndCallMSGraph();
  } catch (error) {
    console.error(error);
  }
}

async function acquireTokenPopupAndCallMSGraph() {
  let tokenResponse;
  try {
    // try and get the token silently in the background
    tokenResponse = await myMSALObj.acquireTokenSilent(requestObj);
  } catch (error) {
    console.log(error);
    // if the silent request failed, it might be because the user
    // needs to request one interactively via a pop-up or redirect
    if (requiresInteraction(error.errorCode)) {
      try {
        // try and get the token with an interactive pop-up window
        tokenResponse = await myMSALObj.acquireTokenPopup(requestObj);
      } catch (error) {
        console.log(error);
      }
    }
  }
  callMSGraph(graphConfig.graphMeEndpoint, tokenResponse.accessToken, graphAPICallback);
}

function graphAPICallback(data) {
  // jsonPre.innerHTML = JSON.stringify(data, null, 2);
  console.error(data);
}

function showWelcomeMessage() {
  userElement.innerHTML = ` (${myMSALObj.getAccount().name})`;
  // Change the login button to log out
  loginButton.removeEventListener('click', signIn, false);
  loginButton.addEventListener('click', signOut, false);
  loginButton.innerHTML = 'Sign Out';
}

// This function can be removed if you do not need to support IE
async function acquireTokenRedirectAndCallMSGraph() {
  try {
    // Always start with acquireTokenSilent to obtain a token in the signed in user from cache
    const tokenResponse = myMSALObj.acquireTokenSilent(requestObj);
    callMSGraph(graphConfig.graphMeEndpoint, tokenResponse.accessToken, graphAPICallback);
  } catch (error) {
    console.log(error);
    // Upon acquireTokenSilent failure (due to consent or interaction or login required ONLY)
    // Call acquireTokenRedirect
    if (requiresInteraction(error.errorCode)) {
      myMSALObj.acquireTokenRedirect(requestObj);
    }
  }
}

function authRedirectCallBack(error, response) {
  //todo refresh?
  if (error) {
    console.log(error);
  } else {
    if (response.tokenType === 'access_token') {
      callMSGraph(graphConfig.graphEndpoint, response.accessToken, graphAPICallback);
    } else {
      console.log('token type is: %s', 0);
    }
    window.location.reload(false);
  }
}

function requiresInteraction(errorCode) {
  if (!errorCode || !errorCode.length) {
    return false;
  }
  return errorCode === 'consent_required' || errorCode === 'interaction_required' || errorCode === 'login_required';
}

// Browser check variables
const ua = window.navigator.userAgent;
const msie = ua.indexOf('MSIE ');
const msie11 = ua.indexOf('Trident/');
const msedge = ua.indexOf('Edge/');
const isIE = msie > 0 || msie11 > 0;
const isEdge = msedge > 0;

// If you support IE, our recommendation is that you sign-in using Redirect APIs
// If you as a developer are testing using Edge InPrivate mode, please add "isEdge" to the if check
// can change this to default an experience outside browser use
const loginType = isIE ? 'REDIRECT' : 'POPUP';

if (loginType === 'POPUP') {
  if (myMSALObj.getAccount()) {
    // avoid duplicate code execution on page load in case of iframe and popup window.
    showWelcomeMessage();
    acquireTokenPopupAndCallMSGraph();
  }
} else if (loginType === 'REDIRECT') {
  signIn.onclick = function() {
    myMSALObj.loginRedirect(requestObj);
  };
  if (myMSALObj.getAccount() && !myMSALObj.isCallback(window.location.hash)) {
    // avoid duplicate code execution on page load in case of iframe and popup window.
    showWelcomeMessage();
    acquireTokenRedirectAndCallMSGraph();
  }
} else {
  console.error('Please set a valid login type');
}

function signOut() {
  myMSALObj.logout();
}

async function callMSGraph(theUrl, accessToken, callback) {
  const xmlHttp = new XMLHttpRequest();
  xmlHttp.onreadystatechange = function() {
    if (this.readyState === 4 && this.status === 200) callback(JSON.parse(this.responseText));
  };
  xmlHttp.open('GET', theUrl, true); // true for asynchronous
  xmlHttp.setRequestHeader('Authorization', `Bearer ${accessToken}`);
  xmlHttp.send();
}
