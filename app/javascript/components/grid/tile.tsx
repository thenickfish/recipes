import React, { Component } from "react";
import { Link } from "react-router-dom";

interface Props {
  name: string;
  description: string;
  link: string;
  index: number;
}
interface State {}

export class Tile extends Component<Props, State> {
  state = {};

  render() {
    return (
      <article
        key={this.props.name}
        className={"style" + ((this.props.index % 15) + 1)}
      >
        <span className="image">
          <img
            src={"/assets/pic0" + ((this.props.index % 15) + 1) + ".jpg"}
            alt=""
          />
        </span>
        <Link to={this.props.link}>
          <h2>{this.props.name}</h2>
          <div className="content">
            <p className="text-transform: uppercase;">
              {this.props.description}
            </p>
          </div>
        </Link>
      </article>
    );
  }
}
