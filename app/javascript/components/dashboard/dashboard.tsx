import React, { Component } from "react";
import Grid from "../grid/grid";
import Category from "./category";

interface Props {}

interface State {
  categories: string[];
}

export default class dashboard extends Component<Props, State> {
  state = {
    categories: [],
  };

  componentDidMount() {
    fetch("/api/v1/categories.json")
      .then((data) => {
        if (data.ok) {
          return data.json();
        }
      })
      .then((data) => {
        data.forEach((element) => {
          const newEl = {
            name: element.name,
            id: element.id,
            description: element.description,
            link: element.slug,
          };
          // this.setState((prevState) => ({
          //   categories: [...prevState, newEl],
          // }))

          this.setState((prevState) => ({
            categories: [...prevState.categories, newEl],
          }));
        });

        // this.setState({ categories: data });
      });
  }

  render() {
    // const categories = [
    //   { name: "foo", description: "idk at all", link: "foo" },
    //   { name: "bar", description: "foo- idk at all", link: "bar" },
    //   { name: "baz", description: "baz - idk at all", link: "baz" },
    // ];
    return <Grid tiles={this.state.categories} />;
  }
}
