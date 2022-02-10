import React, { Component } from "react";
import { Link } from "react-router-dom";

interface Props {
  categoryName: string;
  index: number;
}

interface State {}

export default class Category extends Component<Props, State> {
  state = {};

  constructor(props: Props) {
    super(props);
  }

  render() {
    return (
      <article
        key={this.props.categoryName}
        className={"style" + ((this.props.index % 15) + 1)}
      >
        <span className="image">
          <img
            src={"/assets/pic0" + ((this.props.index % 15) + 1) + ".jpg"}
            alt=""
          />
        </span>
        <Link to={"/categories/" + this.props.categoryName}>
          {/* <a href={"categories/" + this.props.categoryName}> */}
          <h2>{this.props.categoryName}</h2>
          <div className="content">
            <p className="text-transform: uppercase;">test test test blurb</p>
          </div>
          {/* </a> */}
        </Link>
      </article>
    );
  }
}
