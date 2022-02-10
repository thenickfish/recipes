import React, { Component } from "react";

interface Props {}
interface State {}

export default class recipe extends Component<Props, State> {
  state = {};

  render() {
    return (
      <div className="pure-g">
        <div className="pure-u-1 pure-u-md-1-2">
          <div>
            test
            {/* <ul style={list-style: none; text-transform: uppercase}>
        {% for ingredient in page.recipe.ingredients %}
        <li style={padding: 0}>]{ingredient}</li>

        {% endfor %}
      </ul> */}
          </div>
        </div>
      </div>
    );
  }
}
