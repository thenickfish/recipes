import React, { ReactElement } from "react";
import { useParams } from "react-router-dom";
import Grid from "../grid/grid";

interface Props {}

export default function CategoryHome({}: Props): ReactElement {
  let params = useParams();

  let recipes = [
    {
      name: "recipe1",
      description: "recipe 1 desc",
      link: `/${params.categorySlug}/foo`,
    },
  ];
  return (
    <div>
      <Grid tiles={recipes} />
      {JSON.stringify(params)}
    </div>
  );
}
