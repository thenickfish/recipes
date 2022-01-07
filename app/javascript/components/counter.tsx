import React from "react";
import ReactDOM from "react-dom";

type Props = {
  seconds: number;
};

export class Timer extends React.Component<Props, Props> {
  state: Props;
  interval: NodeJS.Timer | undefined;

  constructor(props: Props) {
    super(props);
    this.state = { seconds: props.seconds };
  }

  tick() {
    this.setState((state) => ({
      seconds: state.seconds + 1,
    }));
  }

  componentDidMount() {
    this.interval = setInterval(() => this.tick(), 1000);
  }

  componentWillUnmount() {
    // clearInterval(this.interval);
  }

  render() {
    return <div>Seconds: {this.state.seconds}</div>;
  }
}

// ReactDOM.render(
//     <Timer seconds={0} />,
//     document.getElementById('counter')
// );
