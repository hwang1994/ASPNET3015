import React, { Component } from 'react';
import axios from 'axios';
import Button from 'react-bootstrap/Button';
import Card from 'react-bootstrap/Card';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import { Link } from 'react-router-dom';
import { CopyToClipboard } from 'react-copy-to-clipboard';

class Items extends Component {
    constructor(props) {
        super(props);

        this.state = {
            unpinnedItems: [],
            errorMessage: null,
            recentlyViewedItems: [],
            pinnedItems: [],
            searchText: ''
        };

        this.getAllItems = this.getAllItems.bind(this);
        this.getAllUnpinnedItems = this.getAllUnpinnedItems.bind(this);
        this.deleteItem = this.deleteItem.bind(this);
        this.getAllRecentlyViewedItems = this.getAllRecentlyViewedItems.bind(this);
        this.pinItem = this.pinItem.bind(this);
        this.unpinItem = this.unpinItem.bind(this);
        this.getAllPinnedItems = this.getAllPinnedItems.bind(this);
        this.handleChange = this.handleChange.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
        this.downvoteItem = this.downvoteItem.bind(this);
    }

    componentDidMount() {
        //console.log('-----------------------------------------------------')
        //console.log('MOUNTED');

        window.onpopstate = e => {
            if (window.location.search.includes("term=")) {
                this.setState({
                    searchText: window.location.search.substr(6),
                }, () => {
                    this.getAllItems();
                });
            }
            else {
                this.setState({
                    searchText: '',
                }, () => {
                    this.getAllItems();
                });
            }
        }

        if (window.location.search.includes("term=")) {
            this.setState({
                searchText: window.location.search.substr(6)
            }, () => {
                this.getAllItems();
            });
        }
        else {
            this.getAllItems();
        }
    }

    componentDidUpdate(prevProps) {
        //console.log('Item Component did Update!');
        if (this.props.email !== prevProps.email && this.props.loggedIn != prevProps.loggedIn && this.props.token != prevProps.token) {
            this.getAllUnpinnedItems();
            this.getAllPinnedItems();
        }
        //console.log(this.state);
    }

    getAllItems() {
        this.getAllRecentlyViewedItems();
        this.getAllPinnedItems();
        this.getAllUnpinnedItems();
    }

    getAllUnpinnedItems() {
        const promise = axios.get('items?term=' + this.state.searchText, { withCredentials: true, headers: { 'Authorization': `Bearer ${this.props.token}` } });
        promise
            .then((response) => {
                console.log('items response', response.data);
                if (Array.isArray(response.data)) {
                    this.setState({
                        unpinnedItems: response.data,
                        errorMessage: null
                    });
                }
                else {
                    this.setState({
                        errorMessage: 'no items found'
                    })
                }
            })
            .catch((error) => {
                this.setState({
                    errorMessage: error.toString()
                })
            });
    }

    getAllRecentlyViewedItems() {
        const promise = axios.get('recentlyviewed', { withCredentials: true, headers: { 'Authorization': `Bearer ${this.props.token}` } });
        promise
            .then((response) => {
                console.log('recently viewed response', response.data);
                if (Array.isArray(response.data) && response.data.find(function (el) { return el.user }) !== undefined) {
                    this.setState({
                        recentlyViewedItems: response.data,
                        errorMessage: null
                    });
                }
                else {
                    this.setState({
                        recentlyViewedItems: []
                    })
                }
            })
            .catch((error) => {
                this.setState({
                    errorMessage: error.toString()
                })
            });
    }

    getAllPinnedItems() {
        const promise = axios.get('pins?term=' + this.state.searchText, { withCredentials: true, headers: { 'Authorization': `Bearer ${this.props.token}` } });
        promise
            .then((response) => {
                console.log('pinned response', response.data);
                if (Array.isArray(response.data) && response.data.find(function (el) { return el.item.user }) !== undefined) {
                    this.setState({
                        pinnedItems: response.data,
                        errorMessage: null
                    });
                }
                else {
                    this.setState({
                        pinnedItems: []
                    })
                }
            })
            .catch((error) => {
                console.log(error.toString());
                this.setState({
                    pinnedItems: [],
                    errorMessage: null
                });
            });
    }

    deleteItem(id) {
        axios({
            method: 'delete',
            url: 'items/' + id,
            withCredentials: true,
            headers: {'Authorization': `Bearer ${this.props.token}` }
        })
        .then((response) => {
            console.log('delete response', response.data);
            if (response.data == 'Item deleted!') {
                this.getAllItems();
            }
            else {
                this.props.fail(response.data);
            }
        })
        .catch((error) => {
            this.props.fail(error.toString());
        });
    }

    pinItem(id) {
        axios({
            method: 'post',
            url: 'pins/' + id,
            withCredentials: true,
            headers: { 'Authorization': `Bearer ${this.props.token}` }
        })
        .then((response) => {
            if (response.data == 'Item Pinned') {
                this.getAllPinnedItems();
                this.getAllUnpinnedItems();
            }
        })
        .catch((error) => {
            this.props.fail(error.toString());
        });
    }

    unpinItem(id) {
        axios({
            method: 'delete',
            url: 'pins/' + id,
            withCredentials: true,
            headers: { 'Authorization': `Bearer ${this.props.token}` }
        })
        .then((response) => {
            if (response.data == 'Item unPinned') {
                this.getAllPinnedItems();
                this.getAllUnpinnedItems();
            }
        })
        .catch((error) => {
            this.props.fail(error.toString());
        });
    }

    handleChange(e) {
        this.setState({ searchText: e.target.value });
    }

    handleSubmit(e) {
        //console.log('submitted')
        this.getAllPinnedItems();
        this.getAllUnpinnedItems();
    }

    downvoteItem(id) {
        axios({
            method: 'post',
            url: 'downvotes/' + id,
            withCredentials: true,
            headers: { 'Authorization': `Bearer ${this.props.token}` }
        })
            .then((response) => {
                console.log('downvoted response', response.data);
                if (response.data == 'Downvoted!') {
                    this.props.action();
                }
                else if (response.data == 'Item deleted!') {
                    alert('Downvoted! Now Deleted to due too many downvotes');
                    this.componentDidMount();
                }
                else if (response.data == 'No downvoting more than once on same product!') {
                    this.props.fail(response.data);
                }
            })
            .catch((error) => {
                this.props.fail(error.toString());
            });
    }

    render() {
        const url = window.location.href;
        if (this.state.errorMessage !== null) {
            return (
                <div className="row">
                    <h1 className="login-panel text-muted">{this.state.errorMessage}</h1>
                </div>
            );
        }
        else {
            return (
                <div>
                    <div className="row"><div className="col-md-3"><h2 className="login-panel text-muted">Recently Viewed</h2><hr /></div></div>
                    <Row xs={1} md={4} className="g-4">
                        {this.state.recentlyViewedItems.map(item => {
                            return (
                                <Col key={item.id}>
                                    <Card style={{ width: '18rem', marginRight: '4rem' }}>
                                        <Card.Header>{this.props.email === item.user.email ? <span className="pull-right text-muted"> <Button variant="danger" onClick={e => this.deleteItem(item.id)}><i className="fa fa-trash"></i></Button></span> : <span></span>}</Card.Header>
                                        <Link to={`/product?id=${item.id}`} >
                                            <Card.Img variant="top" src={`pictures/${item.picture}`} />
                                        </Link>
                                        <Card.Body>
                                            <Card.Title>{item.title}</Card.Title>
                                            <Card.Text>{item.description}</Card.Text>
                                            {this.props.loggedIn ? <Button variant="primary" onClick={e => this.downvoteItem(item.id)}><i className="fa fa-thumbs-down"></i></Button> : <span></span>}
                                        </Card.Body>
                                        <Card.Footer className="text-muted"><span><a href={`mailto:${item.user.email}`} data-toggle="tooltip" title="Email seller"><i className="fa fa-envelope"></i>{item.user.firstName + ' ' + item.user.lastName}</a></span> <span className="pull-right">${(Math.round(item.price * 100) / 100).toFixed(2)}</span></Card.Footer>
                                    </Card>
                                </Col>
                            )
                        })}
                    </Row>
                    <br />
                    <div className="row"><div className="col-md-3"><h2 className="login-panel text-muted">Items For Sale</h2><hr /></div></div>
                    <div className="container">
                        <div className="row">
                            <div className="col-md-8">
                                <form className="form-inline" action="#" method="post">
                                    <input type="text" id="search" name="search" className="input-small" placeholder="Search..." value={this.state.searchText} onChange={this.handleChange} />
                                    <Link to={`/?term=${this.state.searchText}`} onClick={this.handleSubmit}><input type="submit" className="btn btn-default" value="Search" /></Link>
                                    <CopyToClipboard text={url}><button className="btn btn-default" data-toggle="tooltip" title="Shareable Link!" onClick={(e) => e.preventDefault()}>Share Link</button></CopyToClipboard>
                                </form>
                            </div>
                        </div>
                    </div>
                    <br />
                    {this.props.loggedIn ?
                        <Row xs={1} md={4} className="g-4">
                            {this.state.pinnedItems.map(pin => {
                                return (
                                    <Col key={pin.id}>
                                        <Card border="warning" style={{ width: '18rem' }}>
                                            <Card.Header><Button variant="warning" onClick={e => this.unpinItem(pin.id)}><i className="fa fa-dot-circle-o"></i></Button>{this.props.email === pin.item.user.email ? <span className="pull-right text-muted"> <Button variant="danger" onClick={e => this.deleteItem(pin.item.id)}><i className="fa fa-trash"></i></Button></span> : <span></span>}</Card.Header>
                                            <Link to={`/product?id=${pin.item.id}`} >
                                                <Card.Img variant="top" src={`pictures/${pin.item.picture}`} />
                                            </Link>
                                            <Card.Body>
                                                <Card.Title>{pin.item.title}</Card.Title>
                                                <Card.Text>{pin.item.description}</Card.Text>
                                                {this.props.loggedIn ? <Button variant="primary" onClick={e => this.downvoteItem(pin.item.id)}><i className="fa fa-thumbs-down"></i></Button> : <span></span>}
                                            </Card.Body>
                                            <Card.Footer className="text-muted"><span><a href={`mailto:${pin.item.user.email}`} data-toggle="tooltip" title="Email seller"><i className="fa fa-envelope"></i>{pin.item.user.firstName + ' ' + pin.item.user.lastName}</a></span> <span className="pull-right">${(Math.round(pin.item.price * 100) / 100).toFixed(2)}</span></Card.Footer>
                                        </Card>
                                    </Col>
                                )
                            })}
                        </Row>
                        : <span></span>
                    }
                    <br />
                    <Row xs={1} md={4} className="g-4">
                        {this.state.unpinnedItems.map(item => {
                            return (
                                <Col key={item.id}>
                                    <Card style={{ width: '18rem' }}>
                                        <Card.Header>{this.props.loggedIn ? <Button variant="warning" onClick={e => this.pinItem(item.id)}><i className="fa fa-thumb-tack"></i></Button> : <span></span>}{this.props.email === item.user.email ? <span className="pull-right text-muted"> <Button variant="danger" onClick={e => this.deleteItem(item.id)}><i className="fa fa-trash"></i></Button></span> : <span></span>}</Card.Header>
                                        <Link to={`/product?id=${item.id}`}>
                                            <Card.Img variant="top" src={`pictures/${item.picture}`} />
                                        </Link>
                                        <Card.Body>
                                            <Card.Title>{item.title}</Card.Title>
                                            <Card.Text>{item.description}</Card.Text>
                                            {this.props.loggedIn ? <Button variant="primary" onClick={e => this.downvoteItem(item.id)}><i className="fa fa-thumbs-down"></i></Button> : <span></span>}
                                        </Card.Body>
                                        <Card.Footer className="text-muted"><span><a href={`mailto:${item.user.email}`} data-toggle="tooltip" title="Email seller"><i className="fa fa-envelope"></i>{item.user.firstName + ' ' + item.user.lastName}</a></span> <span className="pull-right">${(Math.round(item.price * 100) / 100).toFixed(2)}</span></Card.Footer>
                                    </Card>
                                </Col>
                            )
                        })}
                    </Row>
                </div>
            );
        }

    }
}

export default Items;