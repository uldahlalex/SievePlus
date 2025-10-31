import { describe, it, expect, beforeAll } from 'vitest';
import { ComputerStoreClient } from '../src/generated-client';
import type { ComputerQueryModel, SievePlusRequestOfComputerQueryModel } from '../src/generated-client';
import { SievePlusQueryBuilder } from '../../../../ts-sieve-plus-query-builder/src'
import {resolveRefs} from "dotnet-json-refs";

const client = new ComputerStoreClient('http://localhost:5284', {fetch});


  it('should fetch with basic filter on price', async () => {
    const maxPrice = 800;
      
      const query = SievePlusQueryBuilder.create<ComputerQueryModel>()
        .filterGreaterThan("price",maxPrice);

    const sievePlusModel = query.buildSievePlusModel();
      console.log(sievePlusModel)
    const computersResponse = await client.getComputers(sievePlusModel);
    const computers = resolveRefs(computersResponse);
    const prices = computers.map(c => c.price);
      console.log("Prices: "+JSON.stringify(prices))
    const hasItemsWithLowerPriceThanMax = computers.filter(c => c.price <= 800).length > 0;
    if(hasItemsWithLowerPriceThanMax)
        throw new Error("Contained item with price greater than "+maxPrice+": "+prices);
        
  });

it('should fetch with OR filtering', async () => {
    const maxPrice = 800;
    const brand = "Asus";
    
    const allComputers = resolveRefs(await client.getComputers(SievePlusQueryBuilder.create<ComputerQueryModel>().buildSievePlusModel()));
    const brandComputersCheaperThan = allComputers.filter(c => (c.price < maxPrice && c.brand?.name == brand));
    console.dir(brandComputersCheaperThan)
    
    const query = SievePlusQueryBuilder.create<ComputerQueryModel>()
        .filterGreaterThan("price", maxPrice)
        .or()
        .filterContains("brandName", brand);

    const sievePlusModel = query.buildSievePlusModel();
    console.log(sievePlusModel)
    const computersResponse = await client.getComputers(sievePlusModel);
    const computers = resolveRefs(computersResponse);
    
    
});

  // it('should filter computers by brand', async () => {
  //   const request: SievePlusRequestOfComputerQueryModel = {
  //     filters: {
  //       brand: { operator: 'equals', value: 'Dell' }
  //     },
  //     sorts: [],
  //     page: 1,
  //     pageSize: 10
  //   };
  //
  //   const computers = await client.getComputers(request);
  //
  //   expect(computers).toBeDefined();
  //   expect(Array.isArray(computers)).toBe(true);
  //
  //   // All returned computers should be Dell
  //   computers.forEach(computer => {
  //     expect(computer.brand).toBe('Dell');
  //   });
  //
  //   console.log(`Fetched ${computers.length} Dell computers`);
  // });
  //
  // it('should filter computers by price range', async () => {
  //   const request: SievePlusRequestOfComputerQueryModel = {
  //     filters: {
  //       price: { operator: 'greaterThanOrEqual', value: 1000 }
  //     },
  //     sorts: [],
  //     page: 1,
  //     pageSize: 10
  //   };
  //
  //   const computers = await client.getComputers(request);
  //
  //   expect(computers).toBeDefined();
  //   expect(Array.isArray(computers)).toBe(true);
  //
  //   // All returned computers should have price >= 1000
  //   computers.forEach(computer => {
  //     expect(computer.price).toBeGreaterThanOrEqual(1000);
  //   });
  //
  //   console.log(`Fetched ${computers.length} computers with price >= 1000`);
  // });
  //
  // it('should sort computers by price descending', async () => {
  //   const request: SievePlusRequestOfComputerQueryModel = {
  //     filters: {},
  //     sorts: [{ property: 'price', descending: true }],
  //     page: 1,
  //     pageSize: 10
  //   };
  //
  //   const computers = await client.getComputers(request);
  //
  //   expect(computers).toBeDefined();
  //   expect(computers.length).toBeGreaterThan(0);
  //
  //   // Verify sorting - each computer should have price >= next computer
  //   for (let i = 0; i < computers.length - 1; i++) {
  //     expect(computers[i].price).toBeGreaterThanOrEqual(computers[i + 1].price);
  //   }
  //
  //   console.log(`Fetched ${computers.length} computers sorted by price descending`);
  // });
  //
  // it('should support pagination', async () => {
  //   const page1Request: SievePlusRequestOfComputerQueryModel = {
  //     filters: {},
  //     sorts: [{ property: 'id', descending: false }],
  //     page: 1,
  //     pageSize: 5
  //   };
  //
  //   const page2Request: SievePlusRequestOfComputerQueryModel = {
  //     filters: {},
  //     sorts: [{ property: 'id', descending: false }],
  //     page: 2,
  //     pageSize: 5
  //   };
  //
  //   const page1 = await client.getComputers(page1Request);
  //   const page2 = await client.getComputers(page2Request);
  //
  //   expect(page1.length).toBeLessThanOrEqual(5);
  //   expect(page2.length).toBeLessThanOrEqual(5);
  //
  //   // Pages should have different computers
  //   if (page1.length > 0 && page2.length > 0) {
  //     expect(page1[0].id).not.toBe(page2[0].id);
  //   }
  //
  //   console.log(`Page 1: ${page1.length} computers, Page 2: ${page2.length} computers`);
  // });
  //
  // it('should combine filter and sort', async () => {
  //   const request: SievePlusRequestOfComputerQueryModel = {
  //     filters: {
  //       inStock: { operator: 'equals', value: true }
  //     },
  //     sorts: [{ property: 'price', descending: false }],
  //     page: 1,
  //     pageSize: 10
  //   };
  //
  //   const computers = await client.getComputers(request);
  //
  //   expect(computers).toBeDefined();
  //
  //   // All should be in stock
  //   computers.forEach(computer => {
  //     expect(computer.inStock).toBe(true);
  //   });
  //
  //   // Verify ascending price sort
  //   for (let i = 0; i < computers.length - 1; i++) {
  //     expect(computers[i].price).toBeLessThanOrEqual(computers[i + 1].price);
  //   }
  //
  //   console.log(`Fetched ${computers.length} in-stock computers sorted by price ascending`);
  // });
