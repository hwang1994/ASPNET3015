import React, { Component } from 'react';
import axios from 'axios';
import authService from './api-authorization/AuthorizeService'

export class NewItem extends Component {

    constructor(props) {
        super(props);
        this.state = {
            loading: true,
            token : null,
            title: '',
            price: '',
            description: '',
            file: null,
            errorMessage: null
        };

        this.handleSubmit = this.handleSubmit.bind(this);
    }

    componentDidMount() {
        this.populateNewItemForm();
    }

    renderNewItemForm() {
        return (
            <div className="row">
                <div className="col-md-4">
                    {!this.state.errorMessage ? <span></span> : <div className="alert alert-danger text-center">{this.state.errorMessage}<br></br><button type="button" className="btn btn-secondary" onClick={e => this.setState({ errorMessage: null })}>Clear Message</button></div>}
                    <form onSubmit={this.handleSubmit}>
                        <div className="form-group">
                            <label htmlFor="title">Title (only numbers, letters, apotrophes and dashes allowed)</label>
                            <input type="text" className="form-control" id="title" name="title" value={this.state.title} onChange={e => this.setState({ title: e.target.value })} />
                        </div>
                        <br></br>
                        <div className="form-group">
                            <label htmlFor="price">Price</label>
                            <input type="number" className="form-control" min="1" step="any" id="price" name="price" value={this.state.price} onChange={e => this.setState({ price: e.target.value })} />
                        </div>
                        <br></br>
                        <div className="form-group">
                            <label htmlFor="price">Description</label>
                            <input type="text" className="form-control" id="description" name="description" value={this.state.description} onChange={e => this.setState({ description: e.target.value })} />
                        </div>
                        <br></br>
                        <div className="form-group">
                            <label htmlFor="file">Picture (Under 4MB)</label>
                            <input type="file" className="form-control-file" id="file" name="file" onChange={e => this.setState({ file: e.target.files[0] })} />
                        </div>
                        <br></br>
                        <button type="submit" className="btn btn-primary">Submit</button>
                    </form>
                </div>
            </div>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : this.renderNewItemForm();

        return (
            <div>
                <h1 id="tabelLabel" >New Item</h1>
                {contents}
            </div>
        );
    }

    async populateNewItemForm() {
        const token = await authService.getAccessToken();
        if (token) {
            this.setState({
                loading: false,
                token: token
            });
        }
    }

    handleSubmit(event) {
        event.preventDefault();
        console.log(this.state);
        let formData = new FormData();
        formData.append('title', this.state.title)
        formData.append('price', this.state.price)
        formData.append('description', this.state.description)
        formData.append('file', this.state.file)
        axios({
            method: 'post',
            url: 'items',
            data: formData,
            withCredentials: true,
            headers: { 'Content-Type': 'multipart/form-data', 'Authorization': `Bearer ${this.state.token}` } 
        })
        .then((response) => {
            if (response.data == 'Item Uploaded') {
                window.location.href = '/';
            }
            else {
                this.setState({
                    errorMessage: response.data
                });
            }
        })
        .catch((error) => {
            this.setState({
                errorMessage: error.toString()
            });
        });
    }
}
