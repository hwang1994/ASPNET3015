import React, { Component } from 'react';
import axios from 'axios';
import 'bootstrap/dist/css/bootstrap.min.css';
import Card from 'react-bootstrap/Card';
import Button from 'react-bootstrap/Button';
import { Link } from 'react-router-dom';

export class ProductView extends Component {
    constructor(props) {
        super(props);

        this.state = {
            selectedID: window.location.search.substr(4),
            selectedProduct: {
                user: {}
            }
        };

    }

    componentDidMount() {
        //console.log('ProductView Mounted')
        console.log(this.state.selectedID);
        const promise = axios.get('/items/' + this.state.selectedID, { withCredentials: true, });
        promise
            .then((response) => {
                console.log('product view response', response.data);
                if (response.data != 'Product does not exist!' || response.data != 'Error finding Product') {
                    this.setState({
                        selectedProduct: response.data
                    })
                }
            })
            .catch((error) => {
                alert(error.ToString());
            });
        //console.log('END ProductViewdidMount');
    }

    render() {
        if (this.state.selectedProduct.id!==null) {
            return (
                <div className="row">
                    <div className="col-md-6 offset-md-3">
                        <br></br>
                        <Card key={this.state.selectedProduct.id}>
                            <Card.Header><Link to='/'><Button>Back</Button></Link></Card.Header>
                            <Card.Img variant="top" src={`pictures/${this.state.selectedProduct.picture}`} />
                            <Card.Body>
                                <Card.Title>{this.state.selectedProduct.title}</Card.Title>
                                <Card.Text>{this.state.selectedProduct.firstName}</Card.Text>
                            </Card.Body>
                            <Card.Footer className="text-muted"><span><a href={`mailto:${this.state.selectedProduct.email}`} data-toggle="tooltip" title="Email seller"><i className="fa fa-envelope"></i>{this.state.selectedProduct.user.firstName + ' ' + this.state.selectedProduct.user.lastName}</a></span> <span className="pull-right">${(Math.round(this.state.selectedProduct.price * 100) / 100).toFixed(2)}</span></Card.Footer>
                        </Card>
                        <br></br>
                    </div>
                </div>
            );
        }
        else {
            return (
                <div>No Item Found!</div>
            );
        }
    }

}