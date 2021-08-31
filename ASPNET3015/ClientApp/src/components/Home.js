import React, { Component } from 'react';
import Items from './Items.js';
import authService from './api-authorization/AuthorizeService';

export class Home extends Component {
    static displayName = Home.name;

    constructor(props) {
        super(props);

        this.state = {
            loggedIn: false,
            errorMessage: null,
            downvoteMessage: null,
            email: null,
            token: null
        };

        this.downvoteMessage = this.downvoteMessage.bind(this);
        this.setErrorMessage = this.setErrorMessage.bind(this);
    }

    componentDidMount() {
        this._subscription = authService.subscribe(() => this.populateState());
        this.populateState();
    }

    async populateState() {
        const [loggedIn, user, token] = await Promise.all([authService.isAuthenticated(), authService.getUser(), authService.getAccessToken()]);
        this.setState({
            loggedIn,
            email: user && user.name,
            token
        }, () => {
            console.log(this.state);
        });
    }

    downvoteMessage() {
        this.setState({
            downvoteMessage: 'item downvoted'
        });
    }

    setErrorMessage(message) {
        this.setState({
            errorMessage: message
        });
    }

  render () {
    return (
        <div className="App">
            <div className="container">
                <div id="wrapper">
                    <div className="row">
                    <div className="col-md-6 offset-md-3">
                            <h1 className="login-panel text-center text-muted">COMP 3015 Final Project</h1>
                            <hr />
                            {!this.state.errorMessage ? <span></span> : <div className="alert alert-danger text-center">{this.state.errorMessage}<br></br><button type="button" className="btn btn-secondary" onClick={e => this.setState({ errorMessage: null })}>Clear Message</button></div>}
                            {!this.state.downvoteMessage ? <span></span> : <div className="alert alert-success text-center">{this.state.downvoteMessage}<br></br><button type="button" className="btn btn-secondary" onClick={e => this.setState({ downvoteMessage: null })}>Clear Message</button></div>}
                        </div>
                    </div>
                    <Items loggedIn={this.state.loggedIn} email={this.state.email} token={this.state.token} action={this.downvoteMessage} fail={this.setErrorMessage} />
                </div>
            </div>
        </div>
    );
  }
}
