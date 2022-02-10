import React, { Component } from "react";

interface Props {}
interface State {}

export default class menu extends Component<Props, State> {
  state = {};

  render() {
    return (
      <nav id="menu">
        <h2>
          Menu<span id="username"></span>
        </h2>
        <ul>
          <li>
            <a href="{{ '/' | relative_url}}">Home</a>
          </li>
          <li id="editButton">
            <a href="{{ '/edit' | relative_url }}">
              <i className="fas fa-plus-square"></i> Add Recipe
            </a>
          </li>

          <li>
            <a id="loginButton" href="">
              Login
            </a>
          </li>
        </ul>
      </nav>
    );
  }
}
