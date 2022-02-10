import React, { Component } from "react";
import { Tile } from "./tile";

type TileData = {
  name: string;
  description: string;
  link: string;
};

interface Props {
  tiles: Array<TileData>;
}
interface State {}

export default class Grid extends Component<Props, State> {
  state = {};

  render() {
    return (
      <section className="tiles">
        {this.props.tiles.map((data, index) => (
          <Tile
            key={data.name}
            name={data.name}
            description={data.description}
            link={data.link}
            index={index}
          />
        ))}
      </section>
    );
  }
}
