import type {ProblemDetails} from "./problemdetails.ts";
import {ApiException} from "./generated-client.ts";
import toast from "react-hot-toast";

export default function customCatch(e: any) {
    if (e instanceof ApiException) {
        const problemDetails = JSON.parse(e.response) as ProblemDetails;
        toast(problemDetails.title)
    }
}